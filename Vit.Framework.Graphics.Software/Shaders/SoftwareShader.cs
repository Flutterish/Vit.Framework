using System.Diagnostics;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareShader : IShaderPart {
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

	public ShaderPartType Type => Compiler.Bytecode.Type;
	public ShaderInfo ShaderInfo => Compiler.Bytecode.Reflections;

	public void Dispose () {
		
	}
}