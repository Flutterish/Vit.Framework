using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	public readonly Dictionary<uint, PointerVariable> InputsByLocation = new();
	public readonly Dictionary<uint, PointerVariable> OutputsByLocation = new();
	public readonly List<PointerVariable> Outputs = new();
	public readonly Dictionary<uint, IVariable> BuiltinOutputs = new();

	public readonly Dictionary<uint, PointerVariable> InterfacesById = new();
	public readonly Dictionary<uint, PointerVariable> UniformsByBinding = new();
	public readonly Dictionary<uint, IVariable> Constants = new();

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
			var ptr = (PointerVariable)uniform.Type.GetRuntimeType().CreateVariable();
			var value = uniform.Type.Type.GetRuntimeType().CreateVariable();
			ptr.Address = value;

			var binding = uniform.Decorations[DecorationName.Binding].Data[0];
			UniformsByBinding.Add( binding, ptr );
			GlobalScope.Variables.Add( id, ptr );
		}
		foreach ( var i in inputInterfaces ) {
			var location = i.Decorations[DecorationName.Location].Data[0];
			var ptr = (PointerVariable)i.Type.GetRuntimeType().CreateVariable();
			var value = i.Type.Type.GetRuntimeType().CreateVariable();
			ptr.Address = value;

			InputsByLocation.Add( location, ptr );
			InterfacesById.Add( i.Id, ptr );
			GlobalScope.Variables.Add( i.Id, ptr );
		}
		foreach ( var i in outputInterfaces ) {
			var ptr = (PointerVariable)i.Type.GetRuntimeType().CreateVariable();
			var value = i.Type.Type.GetRuntimeType().CreateVariable();
			ptr.Address = value;

			Outputs.Add( ptr );
			InterfacesById.Add( i.Id, ptr );
			GlobalScope.Variables.Add( i.Id, ptr );

			if ( i.Decorations.TryGetValue( DecorationName.Location, out var location ) ) {
				OutputsByLocation.Add( location.Data[0], ptr );
			}

			var innerType = i.Type.Type;
			if ( innerType is StructType structType ) {
				for ( uint j = 0; j < structType.MemberTypeIds.Length; j++ ) {
					if ( structType.GetMemberDecorations( j ).TryGetValue( DecorationName.BuiltIn, out var builtin ) ) {
						BuiltinOutputs.Add( builtin.Data[0], ((ICompositeVariable)value)[j] );
					}
				}
			}
		}

		foreach ( var (id, constant) in compiler.Constants ) {
			var variable = constant.CreateVariable();
			Constants.Add( id, variable );
			GlobalScope.Variables.Add( id, variable );
		}

		foreach ( var (id, constant) in compiler.CompositeConstants ) {
			var variable = constant.CreateVariable();
			Constants.Add( id, variable );
			GlobalScope.Variables.Add( id, variable );
		}

		Entry = new( GlobalScope, entry.Function );
	}

	public void SetUniforms ( uint binding, ReadOnlySpan<byte> data ) {
		if ( !UniformsByBinding.TryGetValue( binding, out var uniform ) )
			return;

		uniform.Address!.Parse( data );
	}
}

public struct ShaderStageOutput {
	public Dictionary<uint, IVariable> Outputs;

	public void Interpolate ( float a, float b, float c, ShaderStageOutput A, ShaderStageOutput B, ShaderStageOutput C ) {
		foreach ( var (id, output) in Outputs ) {
			var _A = A.Outputs[id];
			var _B = B.Outputs[id];
			var _C = C.Outputs[id];

			output.Interpolate( a, b, c, _A, _B, _C );
		}
	}
}

public struct VariableInfo {
	public IRuntimeType Type;
	public int Address;
}

public struct MemoryDebugInfo {
	public VariableInfo Variable;
	public string Name;
}

public ref struct ShaderMemory {
	public Span<byte> Memory;
	public int StackPointer;

	public List<MemoryDebugInfo>? DebugInfo;

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

	public VariableInfo StackAlloc ( IRuntimeType type ) {
		var ptr = StackPointer;
		StackPointer += type.Size;
		return new() { Address = ptr, Type = type };
	}
}