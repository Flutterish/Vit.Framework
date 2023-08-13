using System.Collections.Immutable;
using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Uniforms;
using AddressLinkage = System.Collections.Generic.List<(int ptrAddress, int address)>;
using OutputLinkage = System.Collections.Generic.Dictionary<uint, int>;
using StageVariables = System.Collections.Generic.Dictionary<uint, Vit.Framework.Graphics.Software.Shaders.VariableInfo>;
using UniformLinkage = System.Collections.Generic.Dictionary<(uint set, uint binding), int>;

namespace Vit.Framework.Graphics.Software.Shaders;

public class ShaderSet : IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<SoftwareShader> Shaders;
	public readonly VertexInputDescription InputDescription;

	public ShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) { 
		Shaders = parts.Select( x => (SoftwareShader)x ).ToImmutableArray();

		// TODO this asssumes "vertex -> fragment" shader set
		var vert = Shaders.OfType<SoftwareVertexShader>().Single();
		var frag = Shaders.OfType<SoftwareFragmentShader>().Single();
		Debug.Assert( vertexInput != null );
		InputDescription = vertexInput;

		ShaderMemory memory = default;

		UniformDebugFrame = BakedDebug = new() { Name = "Uniforms" };
		VertexStage.PointerAdresses = new();
		Uniforms = bakeUniforms( ref memory, ref VertexStage );

		VertexDebugFrame = BakedDebug = new() { Name = "Vertex Shader", ParentFrame = BakedDebug, StackPointerOffset = memory.StackPointer };
		(VertexStageLinkage, var vertexOutputs) = bakeVertexOutputs( vert, ref memory, ref VertexStage, "Vert" );
		VertexInputs = bakeInputs( vert, ref memory, ref VertexStage, "Vert", vertexInput );
		VertexStage.StackPointer = memory.StackPointer;

		FragmentDebugFrame = BakedDebug = new() { Name = "Fragment Shader", ParentFrame = BakedDebug, StackPointerOffset = memory.StackPointer };
		FragmentStage.PointerAdresses = new();
		bakeOutputs( frag, ref memory, ref FragmentStage, "Frag" );
		bakeStageLink( frag, ref memory, ref FragmentStage, vertexOutputs, "Frag" );
		FragmentStage.StackPointer = memory.StackPointer;
	}

	// TODO this assumes that shader parts are distinct, we should ensure that they are (compiler being "IShader" and shader set specializing them?)
	UniformLinkage bakeUniforms ( ref ShaderMemory memory, ref BakedStageInfo stageInfo ) {
		UniformLinkage adresses = new();

		foreach ( var (binding, ptrType) in Shaders.SelectMany( x => x.UniformsByBinding ).DistinctBy( x => x.Key ) ) {
			var uniform = memory.StackAlloc( ptrType.Base );
			var ptr = memory.StackAlloc( ptrType );
			stageInfo.PointerAdresses.Add( (ptr.Address, uniform.Address) );

			foreach ( var i in Shaders ) {
				if ( i.UniformIdByBinding.TryGetValue( binding, out var id ) ) {
					i.GlobalScope.VariableInfo[id] = ptr;
				}
			}

			BakedDebug.Add( new() {
				Variable = uniform,
				Name = $"Uniform (Binding {binding.binding} Set {binding.set})"
			} );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Uniform ptr (Binding {binding.binding} Set {binding.set})"
			} );

			adresses.Add(binding, uniform.Address);
		}

		foreach ( var (binding, ptrType) in Shaders.SelectMany( x => x.UniformConstantsByBinding ).DistinctBy( x => x.Key ) ) {
			var uniform = memory.StackAlloc( ptrType.Base );
			var ptr = memory.StackAlloc( ptrType );
			stageInfo.PointerAdresses.Add( (ptr.Address, uniform.Address) );

			foreach ( var i in Shaders ) {
				if ( i.UniformConstantIdByBinding.TryGetValue( binding, out var id ) ) {
					i.GlobalScope.VariableInfo[id] = ptr;
				}
			}

			BakedDebug.Add( new() {
				Variable = uniform,
				Name = $"Uniform Constant (Binding {binding.binding} Set {binding.set})"
			} );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Uniform Constant ptr (Binding {binding.set} Set {binding.set})"
			} );

			adresses.Add(binding, uniform.Address);
		}

		return adresses;
	}

	(VertexLinkage linkage, OutputLinkage outputs) bakeVertexOutputs ( SoftwareVertexShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName ) {
		VertexLinkage linkage = new() {
			Variables = new StageVariables[4],
			PointerAddresses = new()
		};
		OutputLinkage outputs = new();

		for ( int i = 0; i < 4; i++ ) {
			var bakery = linkage.Variables[i] = new();
			foreach ( var (location, ptrType) in shader.OutputsByLocation ) {
				var output = memory.StackAlloc( ptrType.Base );
				bakery.Add(location, output);

				BakedDebug.Add( new() {
					Variable = output,
					Name = $"Out {debugName} (Location {location}) [{(i == 3 ? "Interpolated" : $"Vertex {i}")}]"
				} );

				if ( i != 3 )
					continue;

				outputs.Add(location, output.Address);
			}
		}

		foreach ( var (location, ptrType) in shader.OutputsByLocation ) {
			var ptr = memory.StackAlloc( ptrType );
			var bakery = linkage.PointerAddresses;
			bakery.Add( location, ptr.Address );

			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Out {debugName} ptr (Location {location})"
			} );

			shader.GlobalScope.VariableInfo[shader.OutputIdByLocation[location]] = ptr;
		}

		foreach ( var (output, id) in shader.OutputsWithoutLocation ) {
			var variable = memory.StackAlloc( output.Base );
			BakedDebug.Add( new() {
				Variable = variable,
				Name = $"Out {debugName} (Builtin)"
			} );

			var ptr = memory.StackAlloc( output );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Out {debugName} ptr (Builtin)"
			} );

			stageInfo.PointerAdresses.Add( (ptr.Address, variable.Address) );
			shader.GlobalScope.VariableInfo[id] = ptr;
		}

		return (linkage, outputs);
	}

	StageVariables bakeInputs ( SoftwareShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName, VertexInputDescription vertexInput ) {
		StageVariables inputs = new();

		foreach ( var (location, input) in shader.InputsByLocation ) {
			var variable = memory.StackAlloc( input.Base );
			BakedDebug.Add( new() {
				Variable = variable,
				Name = $"In {debugName} (Location {location})"
			} );

			var ptr = memory.StackAlloc( input );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"In {debugName} ptr (Location {location})"
			} );

			stageInfo.PointerAdresses.Add((ptr.Address, variable.Address));
			shader.GlobalScope.VariableInfo[shader.InputIdByLocation[location]] = ptr;
			inputs.Add( location, variable );
		}

		return inputs;
	}

	void bakeStageLink ( SoftwareShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, OutputLinkage outputAddressByLocation, string debugName ) {
		foreach ( var (location, ptrType) in shader.InputsByLocation ) {
			var ptr = memory.StackAlloc( ptrType );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"In {debugName} ptr (Location {location})"
			} );

			stageInfo.PointerAdresses.Add((ptr.Address, outputAddressByLocation[location]));
			shader.GlobalScope.VariableInfo[shader.InputIdByLocation[location]] = ptr;
		}
	}

	OutputLinkage bakeOutputs ( SoftwareShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName ) {
		var outputs = new OutputLinkage();

		foreach ( var (location, ptrType) in shader.OutputsByLocation ) {
			var output = memory.StackAlloc( ptrType.Base );
			BakedDebug.Add( new() {
				Variable = output,
				Name = $"Out {debugName} (Location {location})"
			} );

			var ptr = memory.StackAlloc( ptrType );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Out {debugName} ptr (Location {location})"
			} );

			stageInfo.PointerAdresses.Add((ptr.Address, output.Address));
			shader.GlobalScope.VariableInfo[shader.OutputIdByLocation[location]] = ptr;
			outputs.Add( location, output.Address );
		}

		return outputs;
	}

	public Dictionary<uint, UniformSet> UniformSets = new();
	public IUniformSet? GetUniformSet ( uint set = 0 ) {
		return UniformSets.GetValueOrDefault( set );
	}

	public IUniformSet CreateUniformSet ( uint set = 0 ) {
		var value = new UniformSet();
		DebugMemoryAlignment.SetDebugData( value, set, this );
		return value;
	}

	public IUniformSetPool CreateUniformSetPool ( uint set, uint size ) {
		return new UniformSetPool( this.CreateUniformSetInfo( set ) );
	}

	public void SetUniformSet ( IUniformSet uniforms, uint set = 0 ) {
		UniformSets[set] = (UniformSet)uniforms;
	}

	public struct BakedStageInfo {
		public AddressLinkage PointerAdresses;
		public int StackPointer;
	}

	public struct VertexLinkage {
		public StageVariables[] Variables;
		public OutputLinkage PointerAddresses;

		public void Interpolate ( float a, float b, float c, ShaderMemory memory ) {
			foreach ( var (location, output) in Variables[3] ) {
				((IInterpolatableRuntimeType)output.Type).Interpolate( a, b, c, Variables[0][location], Variables[1][location], Variables[2][location], output, memory );
			}
		}
	}

	public MemoryDebugFrame BakedDebug;
	public MemoryDebugFrame UniformDebugFrame;
	public MemoryDebugFrame VertexDebugFrame;
	public MemoryDebugFrame FragmentDebugFrame;
	public UniformLinkage Uniforms;
	public StageVariables VertexInputs;
	public BakedStageInfo VertexStage;
	public VertexLinkage VertexStageLinkage;
	public BakedStageInfo FragmentStage;

	public void Dispose () {

	}
}
