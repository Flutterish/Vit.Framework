using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public class Sprite : Drawable {
	Shader shader = null!;
	Texture texture = null!;

	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		shader = deps.Resolve<ShaderStore>().GetShader( new() { Vertex = BasicVertexShader.Identifier, Fragment = BasicFragmentShader.Identifier } );
		texture ??= deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
	}

	SharedResourceInvalidations textureInvalidations;
	public Texture Texture {
		get => texture;
		set {
			if ( texture == value )
				return;

			texture = value;
			textureInvalidations.Invalidate();
			InvalidateDrawNodes();
		}
	}

	ColorRgba<float> tint = ColorRgba.White;
	public ColorRgba<float> Tint {
		get => tint;
		set {
			if ( tint == value )
				return;

			tint = value;
			InvalidateDrawNodes();
		}
	}

	struct Vertex {
		public Point2<float> PositionAndUV;
	}

	struct Uniforms {
		public Matrix4x3<float> Matrix;
		public ColorRgba<float> Tint;
	}

	IUniformSet? uniformSet;
	IDeviceBuffer<ushort>? indices;
	IDeviceBuffer<Vertex>? vertices;
	IHostBuffer<Uniforms>? uniforms;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		uniformSet?.Dispose();
		indices?.Dispose();
		vertices?.Dispose();
		uniforms?.Dispose();
	}

	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	public class DrawNode : DrawableDrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		ColorRgba<float> tint;
		Shader shader = null!;
		Texture texture = null!;
		SharedResourceUpload textureUpload;
		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
			texture = Source.texture;
			tint = Source.tint;
			textureUpload = Source.textureInvalidations.GetUpload();
		}

		void initializeSharedData ( IRenderer renderer ) { // TODO have a global store with basic meshes like this quad
			ref var indices = ref Source.indices;

			if ( indices != null )
				return;

			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;
			var shaders = shader.Value;

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

		public override void Draw ( ICommandBuffer commands ) {
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			var shaders = shader.Value;

			var renderer = commands.Renderer;
			texture.Update( renderer ); // TODO update textures in main draw loop instead

			initializeSharedData( renderer );

			if ( textureUpload.Validate( ref Source.textureInvalidations ) )
				uniformSet!.SetSampler( texture.Value, binding: 1 );
			shaders.SetUniformSet( uniformSet!, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices! );
			commands.BindIndexBuffer( indices! );
			uniforms!.Upload( new Uniforms {
				Matrix = new( UnitToGlobalMatrix ),
				Tint = tint
			} );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
