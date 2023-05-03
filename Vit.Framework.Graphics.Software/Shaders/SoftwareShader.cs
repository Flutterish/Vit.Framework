using System.Diagnostics;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareShader {
	public readonly SpirvCompiler Compiler;
	public readonly ExecutionModel ExecutionModel;

	public readonly Dictionary<uint, RuntimePointerType> InputsByLocation = new();
	public readonly Dictionary<uint, uint> InputIdByLocation = new();
	public readonly Dictionary<uint, RuntimePointerType> OutputsByLocation = new();
	public readonly Dictionary<uint, uint> OutputIdByLocation = new();
	public readonly List<(RuntimePointerType ptr, uint id)> OutputsWithoutLocation = new();
	public readonly List<(RuntimePointerType ptr, uint id)> Outputs = new();
	public readonly Dictionary<uint, int> BuiltinOutputOffsets = new();

	public readonly Dictionary<uint, RuntimePointerType> InterfacesById = new();
	public readonly Dictionary<uint, RuntimePointerType> UniformsByBinding = new();
	public readonly Dictionary<uint, uint> UniformIdByBinding = new();

	public readonly RuntimeScope GlobalScope = new();
	public readonly RuntimeFunction Entry;
	public SoftwareShader ( SpirvCompiler compiler, ExecutionModel model ) {
		Compiler = compiler;
		ExecutionModel = model;

		var entry = compiler.EntryPointsByModel[model];
		var interfaces = entry.InterfaceIds.Select( x => compiler.Variables[x] );
		var inputInterfaces = interfaces.Where( x => x.StorageClass == StorageClass.Input ).ToArray();
		var outputInterfaces = interfaces.Where( x => x.StorageClass == StorageClass.Output ).ToArray();

		foreach ( var (id, uniform) in compiler.Variables.Where( x => x.Value.StorageClass == StorageClass.Uniform ) ) {
			var binding = uniform.Decorations[DecorationName.Binding].Data[0];
			UniformsByBinding.Add( binding, uniform.Type.GetRuntimeType() );
			UniformIdByBinding.Add( binding, id );
		}
		foreach ( var i in inputInterfaces ) {
			var location = i.Decorations[DecorationName.Location].Data[0];

			InputsByLocation.Add( location, i.Type.GetRuntimeType() );
			InputIdByLocation.Add( location, i.Id );
			InterfacesById.Add( i.Id, i.Type.GetRuntimeType() );
		}
		foreach ( var i in outputInterfaces ) {
			Outputs.Add( (i.Type.GetRuntimeType(), i.Id) );
			InterfacesById.Add( i.Id, i.Type.GetRuntimeType() );

			if ( i.Decorations.TryGetValue( DecorationName.Location, out var location ) ) {
				OutputsByLocation.Add( location.Data[0], i.Type.GetRuntimeType() );
				OutputIdByLocation.Add( location.Data[0], i.Id );
			}
			else {
				OutputsWithoutLocation.Add( (i.Type.GetRuntimeType(), i.Id) );
			}

			var innerType = i.Type.Type;
			if ( innerType is StructType structType ) {
				for ( uint j = 0; j < structType.MemberTypeIds.Length; j++ ) {
					if ( structType.GetMemberDecorations( j ).TryGetValue( DecorationName.BuiltIn, out var builtin ) ) {
						BuiltinOutputOffsets.Add( builtin.Data[0], ((ICompositeRuntimeType)structType.GetRuntimeType()).GetMemberOffset( (int)j ) );
					}
				}
			}
		}

		Entry = new( GlobalScope, entry.Function );
	}

	protected void loadConstants ( ref ShaderMemory memory ) {
		foreach ( var (id, constant) in Compiler.Constants ) {
			var variable = memory.StackAlloc( constant.Type.GetRuntimeType() );
			MemoryMarshal.AsBytes( constant.Data.AsSpan() ).CopyTo( memory.Memory[variable.Address..] );
			GlobalScope.VariableInfo[id] = variable;

#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"Constant %{id}"
			} );
#endif
		}

		Debug.Assert( !Compiler.CompositeConstants.Any() );
	}
}

public struct ShaderStageOutput {
	public Dictionary<uint, VariableInfo> OutputsByLocation;

	public void Interpolate ( float a, float b, float c, ShaderStageOutput A, ShaderStageOutput B, ShaderStageOutput C, ShaderMemory memory ) {
		foreach ( var (id, output) in OutputsByLocation ) {
			var _A = A.OutputsByLocation[id];
			var _B = B.OutputsByLocation[id];
			var _C = C.OutputsByLocation[id];

			((IInterpolatableRuntimeType)output.Type).Interpolate( a, b, c, _A, _B, _C, output, memory );
		}
	}
}

[DebuggerDisplay("{ToString(),nq}")]
public struct VariableInfo {
	public IRuntimeType Type;
	public int Address;

	public override string ToString () {
		return $"{Type} at 0x{Address:X}";
	}
}

public struct MemoryDebugInfo {
	public VariableInfo Variable;
	public string Name;
}

public ref struct ShaderMemory {
	public Span<byte> Memory;
	public int StackPointer;

#if SHADER_DEBUG
	public Dictionary<int, MemoryDebugInfo>? DebugInfo;
	public void AddDebug ( MemoryDebugInfo info ) {
		DebugInfo ??= new();
		DebugInfo[info.Variable.Address] = info;
	}
#endif

	public Span<byte> GetMemory ( int offset, int length )
		=> Memory.Slice( offset, length );

	public unsafe T* GetPointer<T> ( int address ) where T : unmanaged {
		return (T*)( Memory.Data() + address );
	}

	public unsafe void* GetPointer ( int address ) {
		return Memory.Data() + address;
	}

	public T Read<T> ( int address ) where T : unmanaged {
		return MemoryMarshal.Read<T>( Memory[address..] );
	}

	public void Write<T> ( int address, T value ) where T : unmanaged {
		MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref value, 1 ) ).CopyTo( Memory[address..] );
	}

	public void Copy ( int from, VariableInfo to ) {
		GetMemory( from, to.Type.Size ).CopyTo( Memory[to.Address..] );
	}

	public void Copy ( VariableInfo from, int to ) {
		GetMemory( from.Address, from.Type.Size ).CopyTo( Memory[to..] );
	}

	public VariableInfo StackAlloc ( IRuntimeType type ) {
		var ptr = StackPointer;
		StackPointer += type.Size;
		return new() { Address = ptr, Type = type };
	}

#if SHADER_DEBUG
	public override string ToString () {
		var memoryLength = Memory.Length.ToString( "X" ).Length;

		var sb = new StringBuilder();
		sb.AppendLine( $"Shader Memory [{Memory.Length}B] ({StackPointer}B used, {Memory.Length - StackPointer}B free)" );
		if ( DebugInfo is null )
			return sb.ToString();

		var stackPtr = StackPointer;
		foreach ( var (address, i) in DebugInfo.Where( x => x.Key < stackPtr ).OrderBy( x => x.Key ) ) {
			string value;
			if ( i.Variable.Type is RuntimePointerType ) {
				value = "0x" + Read<int>( i.Variable.Address ).ToString( "X" ).PadLeft( memoryLength, '0' );
			}
			else {
				var variable = i.Variable.Type.CreateVariable();
				variable.Parse( GetMemory( i.Variable.Address, i.Variable.Type.Size ) );
				value = variable.ToString()!;
			}
			
			sb.AppendLine( $"0x{i.Variable.Address.ToString("X").PadLeft(memoryLength, '0')}\t{i.Name} : {i.Variable.Type} = {value}" );
		}
		return sb.ToString();
	}

	[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
	private List<string> DebugView {
		get {
			List<string> view = new();
			if ( DebugInfo == null )
				return view;

			var memoryLength = Memory.Length.ToString( "X" ).Length;
			var stackPtr = StackPointer;
			foreach ( var (address, i) in DebugInfo.Where( x => x.Key < stackPtr ).OrderBy( x => x.Key ) ) {
				string value;
				if ( i.Variable.Type is RuntimePointerType ) {
					value = "0x" + Read<int>( i.Variable.Address ).ToString( "X" ).PadLeft( memoryLength, '0' );
				}
				else {
					var variable = i.Variable.Type.CreateVariable();
					variable.Parse( GetMemory( i.Variable.Address, i.Variable.Type.Size ) );
					value = variable.ToString()!;
				}

				view.Add( $"0x{i.Variable.Address.ToString( "X" ).PadLeft( memoryLength, '0' )} {i.Name} : {i.Variable.Type} = {value}" );
			}
			return view;
		}
	}
#endif
}