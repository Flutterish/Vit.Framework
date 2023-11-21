using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering.Masking;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.MaskedVertex.Uniforms;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.MaskedVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class TexturedQuad {
	public class DrawDependencies : DisposableObject, IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
		public MaskingDataBuffer Masking = null!;
		public IDeviceBuffer<ushort> Indices = null!;
		public IDeviceBuffer<Vertex> Vertices = null!;

		public IShaderSet Shader = null!;

		public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
			UniformAllocator = new( regionSize: 256, slabSize: 1, renderer, static ( r, s ) => {
				var buffer = r.CreateHostBuffer<Uniforms>( BufferType.Uniform );
				buffer.AllocateUniform( s, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );
				return buffer;
			} );

			var basicShader = dependencies.Resolve<ShaderStore>().GetShader( new() {
				Vertex = new() {
					Shader = MaskedVertex.Identifier,
					Input = MaskedVertex.InputDescription
				},
				Fragment = MaskedFragment.Identifier
			} );
			basicShader.Compile( renderer );
			Shader = basicShader.Value;

			UniformSetAllocator = new( Shader, set: 1, poolSize: 256 );
			var singleUseBuffers = dependencies.Resolve<SingleUseBufferSectionStack>();
			Masking = dependencies.Resolve<MaskingDataBuffer>();
			var indices = singleUseBuffers.AllocateStagingBuffer<ushort>( 6 );
			indices.Upload<ushort>( stackalloc ushort[] {
				0, 1, 2,
				0, 2, 3
			} );

			var vertices = singleUseBuffers.AllocateStagingBuffer<Vertex>( 4 );
			vertices.Upload<Vertex>( stackalloc Vertex[] {
				new() { PositionAndUV = new( 0, 1 ) },
				new() { PositionAndUV = new( 1, 1 ) },
				new() { PositionAndUV = new( 1, 0 ) },
				new() { PositionAndUV = new( 0, 0 ) }
			} );

			using ( var copy = renderer.CreateImmediateCommandBuffer() ) {
				Indices = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
				Indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.CopyBufferRaw( indices.Buffer, Indices, 6 * SizeOfHelper<ushort>.Size, sourceOffset: indices.Offset );

				Vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
				Vertices.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.CopyBufferRaw( vertices.Buffer, Vertices, 4 * SizeOfHelper<Vertex>.Size, sourceOffset: vertices.Offset );
			}
		}

		protected override void Dispose ( bool disposing ) {
			UniformSetAllocator?.Dispose();
			UniformAllocator?.Dispose();
			Indices?.Dispose();
			Vertices?.Dispose();
		}
	}
}
