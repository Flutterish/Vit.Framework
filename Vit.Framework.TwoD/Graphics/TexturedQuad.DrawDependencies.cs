using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Interop;
using Vit.Framework.TwoD.Rendering.Masking;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.MaskedVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class TexturedQuad {
	public class DrawDependencies : IDrawDependency {
		public SingleUseBufferSectionStack BatchAllocator = null!;
		public SingleUseUniformSetPool UniformSetAllocator = null!;
		public MaskingDataBuffer Masking = null!;
		public IDeviceBuffer<ushort> Indices = null!;
		public IDeviceBuffer<Vertex> Vertices = null!;

		public IShaderSet Shader = null!;

		public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
			BatchAllocator = dependencies.Resolve<SingleUseBufferSectionStack>();

			var basicShader = dependencies.Resolve<ShaderStore>().GetShader( new() {
				Vertex = MaskedVertex.Description,
				Fragment = MaskedFragment.Identifier
			} );
			basicShader.Compile( renderer );
			Shader = basicShader.Value;

			UniformSetAllocator = new( Shader, set: 1, poolSize: 256 );
			Masking = dependencies.Resolve<MaskingDataBuffer>();
			var indices = BatchAllocator.AllocateStagingBuffer<ushort>( 6 );
			indices.Upload<ushort>( stackalloc ushort[] {
				0, 1, 2,
				0, 2, 3
			} );

			var vertices = BatchAllocator.AllocateStagingBuffer<Vertex>( 4 );
			vertices.Upload<Vertex>( stackalloc Vertex[] {
				new() { PositionAndUV = new( 0, 1 ) },
				new() { PositionAndUV = new( 1, 1 ) },
				new() { PositionAndUV = new( 1, 0 ) },
				new() { PositionAndUV = new( 0, 0 ) }
			} );

			using ( var copy = renderer.CreateImmediateCommandBuffer() ) {
				Indices = renderer.CreateDeviceBuffer<ushort>( 6, BufferType.Index, BufferUsage.CopyDestination );
				copy.CopyBufferRaw( indices.Buffer, Indices, 6 * SizeOfHelper<ushort>.Size, sourceOffset: indices.Offset );

				Vertices = renderer.CreateDeviceBuffer<Vertex>( 4, BufferType.Vertex, BufferUsage.CopyDestination );
				copy.CopyBufferRaw( vertices.Buffer, Vertices, 4 * SizeOfHelper<Vertex>.Size, sourceOffset: vertices.Offset );
			}
		}

		public void Dispose () {
			UniformSetAllocator?.Dispose();
			Indices?.Dispose();
			Vertices?.Dispose();
		}

		public void EndFrame () {
			UniformSetAllocator.EndFrame();
		}
	}
}
