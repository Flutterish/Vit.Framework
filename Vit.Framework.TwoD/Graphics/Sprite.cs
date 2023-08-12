using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Templates;

namespace Vit.Framework.TwoD.Graphics;

public class Sprite : Drawable {
	Shader shader = null!;
	Texture texture = null!;

	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		drawDependencies = deps.Resolve<DrawDependencies>();
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

	public struct Vertex {
		public Point2<float> PositionAndUV;
	}

	public struct Uniforms {
		public Matrix4x3<float> Matrix;
		public ColorRgba<float> Tint;
	}

	public class DrawDependencies : DisposableObject, IDrawDependency {
		public BufferSectionPool<IHostBuffer<Uniforms>> UniformAllocator = null!;
		public UniformSetPool UniformSetAllocator = null!;
		public IDeviceBuffer<ushort> Indices = null!;
		public IDeviceBuffer<Vertex> Vertices = null!;

		public void Initialize ( IRenderer renderer ) {
			UniformAllocator = new( 256, 1, renderer, static ( r, s ) => {
				var buffer = r.CreateHostBuffer<Uniforms>( BufferType.Uniform );
				buffer.Allocate( s, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );
				return buffer;
			} );

			UniformSetAllocator = new( new[] {
				BasicVertexShader.Spirv.Reflections,
				BasicFragmentShader.Spirv.Reflections
			}.CreateUniformSetInfo( set: 1 ), renderer, 256 );

			using var copy = renderer.CreateImmediateCommandBuffer();
			Indices = renderer.CreateDeviceBuffer<ushort>( BufferType.Index );
			Indices.Allocate( 6, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
			copy.Upload( Indices, new ushort[] {
				0, 1, 2,
				0, 2, 3
			} );

			Vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
			Vertices.Allocate( 4, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
			copy.Upload( Vertices, new Vertex[] {
				new() { PositionAndUV = new( 0, 1 ) },
				new() { PositionAndUV = new( 1, 1 ) },
				new() { PositionAndUV = new( 1, 0 ) },
				new() { PositionAndUV = new( 0, 0 ) }
			} );
		}

		protected override void Dispose ( bool disposing ) {
			UniformSetAllocator?.Dispose();
			UniformAllocator?.Dispose();
			Indices?.Dispose();
			Vertices?.Dispose();
		}
	}

	DrawDependencies drawDependencies = null!;
	bool areUniformsInitialized = false;
	UniformSetPool.Allocation uniformSet;
	BufferSectionPool<IHostBuffer<Uniforms>>.Allocation uniforms;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		if ( !areUniformsInitialized )
			return;

		drawDependencies.UniformSetAllocator.Free( uniformSet );
		drawDependencies.UniformAllocator.Free( uniforms );
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

		void initializeSharedData () {
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			uniforms = Source.drawDependencies.UniformAllocator.Allocate();
			uniformSet = Source.drawDependencies.UniformSetAllocator.Allocate();

			uniformSet.UniformSet.SetUniformBuffer( uniforms.Buffer, binding: 0, uniforms.Offset );
		}

		public override void Draw ( ICommandBuffer commands ) {
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet.UniformSet;

			var shaders = shader.Value;

			if ( !Source.areUniformsInitialized ) {
				initializeSharedData();
				Source.areUniformsInitialized = true;
			}

			var indices = Source.drawDependencies.Indices;
			var vertices = Source.drawDependencies.Vertices;

			if ( textureUpload.Validate( ref Source.textureInvalidations ) )
				uniformSet.SetSampler( texture.Value, binding: 1 );
			shaders.SetUniformSet( uniformSet, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices );
			commands.BindIndexBuffer( indices );
			uniforms.Buffer.Upload( new Uniforms { // in theory we could make the matrix and tint per-instance to both save space on uniform buffers and batch sprites
				Matrix = new( UnitToGlobalMatrix ),
				Tint = tint
			}, uniforms.Offset );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
