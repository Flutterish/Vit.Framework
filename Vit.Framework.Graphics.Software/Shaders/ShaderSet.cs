using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;
using OutputLinkage = System.Collections.Generic.Dictionary<uint, int>;
using AddressLinkage = System.Collections.Generic.List<(int ptrAddress, int address)>;
using StageVariables = System.Collections.Generic.Dictionary<uint, Vit.Framework.Graphics.Software.Shaders.VariableInfo>;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Software.Uniforms;

namespace Vit.Framework.Graphics.Software.Shaders;

public class ShaderSet : IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<SoftwareShader> Shaders;

	public ShaderSet ( IEnumerable<IShaderPart> parts ) { 
		Shaders = parts.Select( x => (SoftwareShader)x ).ToImmutableArray();

		// TODO this asssumes "vertex -> fragment" shader set
		var vert = Shaders.OfType<SoftwareVertexShader>().Single();
		var frag = Shaders.OfType<SoftwareFragmentShader>().Single();

		frag.GlobalScope.Opaques.Samplers = ((UniformSet)GetUniformSet(0)).Samplers;

		ShaderMemory memory = default;

		UniformDebugFrame = BakedDebug = new() { Name = "Uniforms" };
		VertexStage.PointerAdresses = new();
		Uniforms = bakeUniforms( ref memory, ref VertexStage );

		VertexDebugFrame = BakedDebug = new() { Name = "Vertex Shader", ParentFrame = BakedDebug, StackPointerOffset = memory.StackPointer };
		(VertexStageLinkage, var vertexOutputs) = bakeVertexOutputs( vert, ref memory, ref VertexStage, "Vert" );
		VertexInputs = bakeInputs( vert, ref memory, ref VertexStage, "Vert" );
		VertexStage.StackPointer = memory.StackPointer;

		FragmentDebugFrame = BakedDebug = new() { Name = "Fragment Shader", ParentFrame = BakedDebug, StackPointerOffset = memory.StackPointer };
		FragmentStage.PointerAdresses = new();
		bakeOutputs( frag, ref memory, ref FragmentStage, "Frag" );
		bakeStageLink( frag, ref memory, ref FragmentStage, vertexOutputs, "Frag" );
		FragmentStage.StackPointer = memory.StackPointer;
	}

	// TODO this assumes that shader parts are distinct, we should ensure that they are (compiler being "IShader" and shader set specializing them?)
	OutputLinkage bakeUniforms ( ref ShaderMemory memory, ref BakedStageInfo stageInfo ) {
		OutputLinkage adresses = new();

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
				Name = $"Uniform (Binding {binding})"
			} );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Uniform ptr (Binding {binding})"
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
				Name = $"Uniform Constant (Binding {binding})"
			} );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Uniform Constant ptr (Binding {binding})"
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

	StageVariables bakeInputs ( SoftwareShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName ) {
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

	public Dictionary<uint, UniformSet> uniformSets = new();
	public IUniformSet GetUniformSet ( uint set = 0 ) {
		if ( !uniformSets.TryGetValue( set, out var value ) )
			uniformSets.Add( set, value = new() );

		return value;
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
	public OutputLinkage Uniforms;
	public StageVariables VertexInputs;
	public BakedStageInfo VertexStage;
	public VertexLinkage VertexStageLinkage;
	public BakedStageInfo FragmentStage;

	public void Dispose () {

	}
}
