using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.Graphics.TwoD.Text;

public class SpriteText : Drawable, IDrawableLayoutElement { // TODO this is a scam and is actually just a bunch of vertices
	Font font = null!;
	public Font Font { get => font; init => font = value; }
	public FontIdentifier? FontId { get; init; }
	public required float FontSize { get; init; }
	public required string Text { get; init; }
	public required ColorRgba<float> Tint { get; init; }

	public Size2<float> Size { get; set; }
	public Size2<float> RequiredSize { get; }

	Shader shader = null!;
	Texture texture = null!;
	protected override void Load ( IReadOnlyDependencyCache deps ) {
		base.Load( deps );

		shader = deps.Resolve<ShaderStore>().GetShader( new() { Vertex = DrawNodeRenderer.TestVertex, Fragment = DrawNodeRenderer.TestFragment } );
		texture = deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
		font ??= deps.Resolve<FontStore>().GetFont( FontId ?? FontStore.DefaultFont );
	}

	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
	}

	struct Vertex {
		public Point2<float> PositionAndUV;
	}

	struct Uniforms {
		public Matrix4x3<float> Matrix;
		public ColorRgba<float> Tint;
	}

	IUniformSet? uniformSet;
	IDeviceBuffer<uint>? indices;
	IDeviceBuffer<Vertex>? vertices;
	IHostBuffer<Uniforms>? uniforms;
	int indexCount;
	public class DrawNode : BasicDrawNode<SpriteText> {
		public DrawNode ( SpriteText source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		Shader shader = null!;
		Texture texture = null!;
		Font font = null!;
		string text = null!;
		float size;
		ColorRgba<float> tint;
		protected override void UpdateState () {
			base.UpdateState();
			shader = Source.shader;
			texture = Source.texture;
			font = Source.Font;
			text = Source.Text;
			size = Source.FontSize / (float)font.UnitsPerEm;
			tint = Source.Tint;
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
				List<Vertex> verticesList = new();
				List<uint> indicesList = new();

				var advance = Vector2<float>.Zero;
				foreach ( var rune in text.EnumerateRunes() ) {
					var glyph = font.GetGlyph( rune );
					foreach ( var spline in glyph.Outline.Splines ) {
						uint? _anchor = null;
						uint? _last = null;
						foreach ( var p in spline.GetPoints() ) {
							var point = p.Cast<float>();
							var index = (uint)verticesList.Count;
							verticesList.Add( new() { PositionAndUV = point + advance } );
							
							if ( _anchor is not uint anchor ) {
								_anchor = index;
								continue;
							}
							if ( _last is not uint last ) {
								_last = index;
								continue;
							}

							indicesList.AddRange( new[] { anchor, last, index } );
							_last = index;
						}
					}

					advance.X += (float)glyph.HorizontalAdvance;
				}

				Source.indexCount = indicesList.Count;
				indices = renderer.CreateDeviceBuffer<uint>( BufferType.Index );
				indices.Allocate( (uint)indicesList.Count, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( indices, indicesList.AsSpan() );
				vertices = renderer.CreateDeviceBuffer<Vertex>( BufferType.Vertex );
				vertices.Allocate( (uint)verticesList.Count, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame );
				copy.Upload( vertices, verticesList.AsSpan() );
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
				Matrix = new( Matrix3<float>.CreateScale( size, size ) * UnitToGlobalMatrix ),
				Tint = tint
			} );

			using ( commands.PushDepthTest( new( CompareOperation.Never ), new() { WriteOnPass = false } ) ) {
				using ( commands.PushStencilTest( new( CompareOperation.Always ), new() { 
					CompareMask = 1u,
					WriteMask = 1u,
					DepthFailOperation = StencilOperation.Invert
				} ) ) {
					commands.DrawIndexed( (uint)Source.indexCount );
				}
			}

			using ( commands.PushStencilTest( new( CompareOperation.Equal ), new( StencilOperation.SetTo0 ) { CompareMask = 1u, ReferenceValue = 1u } ) ) {
				commands.DrawIndexed( (uint)Source.indexCount );
			}
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
