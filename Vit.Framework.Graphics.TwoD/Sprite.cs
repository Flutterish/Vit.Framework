using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public class Sprite : Drawable {
	Shader shader;
	public Sprite ( ShaderStore shaders ) {
		shader = shaders.GetShader( new() { Vertex = DrawableRenderer.TestVertex, Fragment = DrawableRenderer.TestFragment } );
	}

	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	new public class DrawNode : BasicDrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
		}

		struct Vertex {
			public Point2<float> PositionAndUV;
		}

		struct Uniforms { // TODO we need a debug check for memory alignment in these
			public Matrix4x3<float> Matrix;
		}

		Shader shader = null!;
		IDeviceBuffer<ushort>? indices;
		IDeviceBuffer<Vertex>? vertices;
		IHostBuffer<Uniforms>? uniforms;
		public override void Draw ( ICommandBuffer commands ) {
			var shaders = shader.Value;

			if ( indices == null ) {
				var renderer = commands.Renderer;
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

				shaders.SetUniformBuffer( uniforms, binding: 0 );
			}

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices! );
			commands.BindIndexBuffer( indices! );
			var mat = UnitToGlobalMatrix * new Matrix3<float>( commands.Renderer.CreateLeftHandCorrectionMatrix<float>() );
			uniforms!.Upload( new Uniforms { 
				Matrix = new( mat )
			} );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) {
			if ( indices == null )
				return;

			indices!.Dispose();
			vertices!.Dispose();
			uniforms!.Dispose();

			indices = null;
			vertices = null;
			uniforms = null;
		}
	}
}
