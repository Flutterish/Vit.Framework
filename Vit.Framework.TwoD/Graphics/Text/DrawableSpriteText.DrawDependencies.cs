using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using static Vit.Framework.TwoD.Rendering.Shaders.BasicVertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public partial class DrawableSpriteText {
	public class DrawDependencies : DisposableObject, IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
		public SpriteFontStore Store = null!;

		public IShaderSet Shader = null!;

		public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
			Store = dependencies.Resolve<SpriteFontStore>();

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
		}

		protected override void Dispose ( bool disposing ) {
			UniformSetAllocator?.Dispose();
			UniformAllocator?.Dispose();
		}
	}
}
