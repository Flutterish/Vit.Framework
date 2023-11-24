using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.TwoD.Rendering.Masking;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using static Vit.Framework.TwoD.Rendering.Shaders.TextVertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public partial class DrawableSpriteText {
	public class DrawDependencies : IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
		public SpriteFontStore Store = null!;
		public SingleUseBufferSectionStack SingleUseBuffers = null!;
		public DeviceBufferHeap BufferHeap = null!;
		public MaskingDataBuffer Masking = null!;

		public IShaderSet Shader = null!;
		public IDeviceBuffer<TextVertex.Corner> CornerBuffer = null!;
		public IDeviceBuffer<ushort> IndexBuffer = null!;

		public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
			Store = dependencies.Resolve<SpriteFontStore>();
			SingleUseBuffers = dependencies.Resolve<SingleUseBufferSectionStack>();
			BufferHeap = dependencies.Resolve<DeviceBufferHeap>();
			Masking = dependencies.Resolve<MaskingDataBuffer>();

			UniformAllocator = new( regionSize: 256, slabSize: 1, renderer, static ( r, s ) => {
				var buffer = r.CreateUniformHostBuffer<Uniforms>( s, BufferType.Uniform, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );
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

			CornerBuffer = renderer.CreateDeviceBuffer<TextVertex.Corner>( 4, BufferType.Vertex, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );
			IndexBuffer = renderer.CreateDeviceBuffer<ushort>( 6, BufferType.Index, BufferUsage.GpuRead | BufferUsage.GpuWrite | BufferUsage.GpuPerFrame );

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

		public void Dispose () {
			CornerBuffer?.Dispose();
			IndexBuffer?.Dispose();
			UniformSetAllocator?.Dispose();
			UniformAllocator?.Dispose();
		}
	}
}
