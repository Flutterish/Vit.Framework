using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Spirv.Instructions;
using OutputLinkage = System.Collections.Generic.Dictionary<uint, int>;
using AddressLinkage = System.Collections.Generic.List<(int ptrAddress, int address)>;

namespace Vit.Framework.Graphics.Software.Shaders;

public class ShaderSet : IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<SoftwareShader> Shaders;

	public ShaderSet ( IEnumerable<IShaderPart> parts ) { 
		Shaders = parts.Select( x => (SoftwareShader)x ).ToImmutableArray();

		// TODO this asssumes "vertex -> fragment" shader set
		var vert = Shaders.OfType<SoftwareVertexShader>().Single();
		var frag = Shaders.OfType<SoftwareFragmentShader>().Single();

		ShaderMemory memory = default;
		VertexStage.PointerAdresses = new();
		bakeUniforms( ref memory, ref VertexStage );
		(VertexStageLinkage, var vertexOutputs) = bakeVertexOutputs( vert, ref memory, ref VertexStage, "Vert" );
		bakeInputs( vert, ref memory, ref VertexStage, "Vert" );
		VertexStage.StackPointer = memory.StackPointer;

		FragmentStage.PointerAdresses = new();
		bakeOutputs( frag, ref memory, ref FragmentStage, "Frag" );
		bakeStageLink( frag, ref memory, ref FragmentStage, vertexOutputs, "Frag" );
		FragmentStage.StackPointer = memory.StackPointer;
	}

	// TODO this assumes that shader parts are distinct, we should ensure that they are (compiler being "IShader" and shader set specializing them?)
	void bakeUniforms ( ref ShaderMemory memory, ref BakedStageInfo stageInfo ) {
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
				Name = $"Uniform (Set {binding})"
			} );
			BakedDebug.Add( new() {
				Variable = ptr,
				Name = $"Uniform ptr (Set {binding})"
			} );
		}
	}

	(AddressLinkage[] linkage, OutputLinkage outputs) bakeVertexOutputs ( SoftwareVertexShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName ) {
		AddressLinkage[] linkage = new AddressLinkage[4];
		OutputLinkage outputs = new();

		for ( int i = 0; i < 4; i++ ) {
			var bakery = linkage[i] = new();
			foreach ( var (location, ptrType) in shader.OutputsByLocation ) {
				var output = memory.StackAlloc( ptrType.Base );
				bakery.Add(( -1, output.Address ));

				BakedDebug.Add( new() {
					Variable = output,
					Name = $"Out {debugName} (Location {location}) [{(i == 3 ? "Interpolated" : $"Vertex {i}")}]"
				} );

				if ( i != 3 )
					continue;

				outputs.Add(location, output.Address);
			}
		}

		int j = 0;
		foreach ( var (location, ptrType) in shader.OutputsByLocation ) {
			var ptr = memory.StackAlloc( ptrType );
			var bakery = linkage[3];
			bakery[j] = bakery[j++] with { ptrAddress = ptr.Address };

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

	void bakeInputs ( SoftwareShader shader, ref ShaderMemory memory, ref BakedStageInfo stageInfo, string debugName ) {
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
		}
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

	public readonly Dictionary<uint, (IByteBuffer buffer, uint stride, uint offset)> UniformBuffers = new();
	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		var vertex = Shaders.First( x => x.Type == ShaderPartType.Vertex );
		UniformBuffers[binding] = ((IByteBuffer)buffer, IBuffer<T>.Stride, offset * IBuffer<T>.Stride);
	}

	public struct BakedStageInfo {
		public AddressLinkage PointerAdresses;
		public int StackPointer;
	}

	public MemoryDebugFrame BakedDebug = new();
	public BakedStageInfo VertexStage;
	public AddressLinkage[] VertexStageLinkage;
	public BakedStageInfo FragmentStage;

	public void Dispose () {

	}
}
