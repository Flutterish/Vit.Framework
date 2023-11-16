using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using Uniforms = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Uniforms;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.BasicVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class TexturedQuad {
	public class DrawDependencies : DisposableObject, IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
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
					Shader = BasicVertex.Identifier,
					Input = BasicVertex.InputDescription
				},
				Fragment = BasicFragment.Identifier
			} );
			basicShader.Compile( renderer );
			Shader = basicShader.Value;

			UniformSetAllocator = new( Shader, set: 1, poolSize: 256 );

			using var indexSource = renderer.CreateStagingBuffer<ushort>();
			indexSource.Allocate( 6, BufferUsage.GpuRead | BufferUsage.GpuRarely | BufferUsage.CpuWrite | BufferUsage.CpuRarely );
			indexSource.Upload( stackalloc ushort[] {
				0, 1, 2,
				0, 2, 3
			} );

			using var vertexSource = renderer.CreateStagingBuffer<Vertex>();
			vertexSource.Allocate( 4, BufferUsage.GpuRead | BufferUsage.GpuRarely | BufferUsage.CpuWrite | BufferUsage.CpuRarely );
			vertexSource.Upload( stackalloc Vertex[] {
				new() { PositionAndUV = new( 0, 1 ) },
				new() { PositionAndUV = new( 1, 1 ) },
				new() { PositionAndUV = new( 1, 0 ) },
				new() { PositionAndUV = new( 0, 0 ) }
			} );

			using ( var copy = renderer.CreateImmediateCommandBuffer() ) {
				Indices = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
				Indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.CopyBuffer( indexSource, Indices, 6 );

				Vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
				Vertices.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.CopyBuffer( vertexSource, Vertices, 4 );
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
