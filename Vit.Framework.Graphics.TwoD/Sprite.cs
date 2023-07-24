﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD.Input.Events;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public class Sprite : Drawable, ILayoutElement, IEventHandler<HoveredEvent> {
	Shader shader = null!;
	Texture texture = null!;

	public bool OnEvent ( HoveredEvent @event ) {
		return true;
	}

	protected override void Load ( IReadOnlyDependencyCache deps ) {
		base.Load( deps );

		shader = deps.Resolve<ShaderStore>().GetShader( new() { Vertex = DrawableRenderer.TestVertex, Fragment = DrawableRenderer.TestFragment } );
		texture ??= deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
	}

	public Size2<float> Size {
		get => new(Scale);
		set => Scale = new(value);
	}
	public Size2<float> RequiredSize => Size2<float>.Zero;

	public Texture Texture {
		get => texture;
		set {
			if ( texture == value )
				return;

			texture = value;
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
	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	new public class DrawNode : BasicDrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		ColorRgba<float> tint;
		Shader shader = null!;
		Texture texture = null!;
		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
			texture = Source.texture;
			tint = Source.tint;
		}

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
				Matrix = new( UnitToGlobalMatrix ),
				Tint = tint
			} );
			commands.DrawIndexed( 6 );
		}

		public override void ReleaseResources ( bool willBeReused ) {
			if ( Source.indices == null )
				return;

			ref var indices = ref Source.indices!;
			ref var vertices = ref Source.vertices!;
			ref var uniforms = ref Source.uniforms!;
			ref var uniformSet = ref Source.uniformSet!;

			indices.Dispose();
			vertices.Dispose();
			uniforms.Dispose();
			uniformSet.Dispose();

			indices = null;
			vertices = null;
			uniforms = null;
			uniformSet = null;
		}
	}
}
