using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public class Sprite : Drawable {
	Shader shader = null!;
	Texture texture = null!;

	protected override void Load () {
		var deps = Parent!.Dependencies;

		shader = deps.Resolve<ShaderStore>().GetShader( new() { Vertex = DrawableRenderer.TestVertex, Fragment = DrawableRenderer.TestFragment } );
		texture ??= deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
	}

	public Texture Texture {
		get => texture;
		set {
			if ( texture == value )
				return;

			texture = value;
			InvalidateDrawNodes();
		}
	}

	struct Vertex {
		public Point2<float> PositionAndUV;
	}

	struct Uniforms {
		public Matrix4x3<float> Matrix;
	}

	IUniformSet? uniformSet;
	IDeviceBuffer<ushort>? indices;
	IDeviceBuffer<Vertex>? vertices;
	IHostBuffer<Uniforms>? uniforms;
	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
		throw new NotImplementedException();
	}

	new public class DrawNode : BasicDrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
			texture = Source.texture;
		}

		Shader shader = null!;
		Texture texture = null!;
		public override void Draw ( ICommandBuffer commands ) {
			var shaders = shader.Value;
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			var renderer = commands.Renderer;
			texture.Update( renderer );

			if ( indices == null ) {
				using var copy = renderer.CreateImmediateCommandBuffer();
				indices = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
				indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( indices, new ushort[] {
					0, 1, 2,
					0, 2, 3
				} );
				vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
				vertices.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( vertices, new Vertex[] {
					new() { PositionAndUV = new( 0, 1 ) },
					new() { PositionAndUV = new( 1, 1 ) },
					new() { PositionAndUV = new( 1, 0 ) },
					new() { PositionAndUV = new( 0, 0 ) }
				} );
				uniforms = renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
				uniforms.Allocate( 1, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );

				uniformSet = shaders.CreateUniformSet( set: 1 );
				uniformSet.SetUniformBuffer( uniforms, binding: 0 );
			}
			uniformSet!.SetSampler( texture.Value, binding: 1 );
			shaders.SetUniformSet( uniformSet, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices! );
			commands.BindIndexBuffer( indices! );
			uniforms!.Upload( new Uniforms { 
				Matrix = new( UnitToGlobalMatrix )
			} );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) {
			
		}
	}
}
