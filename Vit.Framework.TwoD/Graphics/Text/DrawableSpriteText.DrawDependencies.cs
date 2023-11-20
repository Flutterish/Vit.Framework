using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using static Vit.Framework.TwoD.Rendering.Shaders.TextVertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public partial class DrawableSpriteText {
	public class DrawDependencies : DisposableObject, IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
		public SpriteFontStore Store = null!;
		public SingleUseBufferSectionStack SingleUseBuffers = null!;
		public DeviceBufferHeap BufferHeap = null!;

		public IShaderSet Shader = null!;
		public IDeviceBuffer<TextVertex.Corner> CornerBuffer = null!;
		public IDeviceBuffer<ushort> IndexBuffer = null!;

		public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
			Store = dependencies.Resolve<SpriteFontStore>();
			SingleUseBuffers = dependencies.Resolve<SingleUseBufferSectionStack>();
			BufferHeap = dependencies.Resolve<DeviceBufferHeap>();

			UniformAllocator = new( regionSize: 256, slabSize: 1, renderer, static ( r, s ) => {
				var buffer = r.CreateHostBuffer<Uniforms>( BufferType.Uniform );
				buffer.AllocateUniform( s, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );
				return buffer;
			} );

			var basicShader = dependencies.Resolve<ShaderStore>().GetShader( new() {
				Vertex = new() {
					Shader = TextVertex.Identifier,
					Input = TextVertex.InputDescription
				},
				Fragment = TextFragment.Identifier
			} );
			basicShader.Compile( renderer );
			Shader = basicShader.Value;

			UniformSetAllocator = new( Shader, set: 1, poolSize: 256 );

			CornerBuffer = renderer.CreateDeviceBuffer<TextVertex.Corner>( BufferType.Vertex );
			CornerBuffer.Allocate( 4, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );
			IndexBuffer = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
			IndexBuffer.Allocate( 6, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );

			using var copy = renderer.CreateImmediateCommandBuffer();
			var cornerStaging = SingleUseBuffers.AllocateStagingBuffer<TextVertex.Corner>( 4 );
			var indexStaging = SingleUseBuffers.AllocateStagingBuffer<ushort>( 6 );

			cornerStaging.Upload( stackalloc TextVertex.Corner[] {
				new() { Value = (0, 1) },
				new() { Value = (1, 1) },
				new() { Value = (1, 0) },
				new() { Value = (0, 0) }
			} );
			indexStaging.Upload( stackalloc ushort[] {
				0, 1, 2,
				0, 2, 3
			} );

			copy.CopyBufferRaw( cornerStaging.Buffer, CornerBuffer, cornerStaging.Length, sourceOffset: cornerStaging.Offset );
			copy.CopyBufferRaw( indexStaging.Buffer, IndexBuffer, indexStaging.Length, sourceOffset: indexStaging.Offset );
		}

		protected override void Dispose ( bool disposing ) {
			UniformSetAllocator?.Dispose();
			UniformAllocator?.Dispose();
		}
	}
}
