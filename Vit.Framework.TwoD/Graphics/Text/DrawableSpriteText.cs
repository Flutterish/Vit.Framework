using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics.Text;

public class DrawableSpriteText : Drawable { // TODO this is a scam and is actually just a bunch of vertices
	Font font = null!;
	public Font Font {
		get => font;
		set {
			if ( value.TrySet( ref font ) )
				invalidateFontMesh();
		}
	}
	float fontSize = 16;
	public float FontSize {
		get => fontSize;
		set {
			if ( value.TrySet( ref fontSize ) )
				InvalidateDrawNodes();
		}
	}
	string text = string.Empty;
	public string Text {
		get => text;
		set {
			if ( value.TrySet( ref text ) )
				invalidateFontMesh();
		}
	}
	ColorRgba<float> tint = ColorRgba.Black;
	public ColorRgba<float> Tint {
		get => tint;
		set {
			if ( value.TrySet( ref tint ) )
				InvalidateDrawNodes();
		}
	}

	Shader shader = null!;
	Texture texture = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );

		shader = deps.Resolve<ShaderStore>().GetShader( new() {
			Vertex = new() {
				Shader = BasicVertexShader.Identifier,
				Input = BasicVertexShader.InputDescription
			},
			Fragment = BasicFragmentShader.Identifier
		} );
		texture = deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
		font ??= deps.Resolve<FontStore>().GetFont( FontStore.DefaultFont );
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

	void invalidateFontMesh () {
		textMesh.Invalidate();
		InvalidateDrawNodes();
	}
	SharedResourceInvalidations textMesh;

	IUniformSet? uniformSet;
	StagedDeviceBuffer<uint>? indices;
	StagedDeviceBuffer<Vertex>? vertices;
	IHostBuffer<Uniforms>? uniforms;
	int indexCount;

	public override void DisposeDrawNodes () {
		base.DisposeDrawNodes();

		uniformSet?.Dispose();
		indices?.Dispose();
		vertices?.Dispose();
		uniforms?.Dispose();
	}

	public class DrawNode : DrawableDrawNode<DrawableSpriteText> {
		public DrawNode ( DrawableSpriteText source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		SharedResourceUpload textMeshUpload;
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
			textMeshUpload = Source.textMesh.GetUpload();
		}

		void updateTextMesh ( IRenderer renderer ) {
			var shaders = shader.Value;
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			if ( indices != null ) {
				indices.Dispose();
				vertices!.Dispose();
				uniforms!.Dispose();
				uniformSet!.Dispose();
			}

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
			indices = new( renderer, BufferType.Index );
			vertices = new( renderer, BufferType.Vertex );
			uniforms = renderer.CreateHostBuffer<Uniforms>( BufferType.Uniform );
			uniformSet = shaders.CreateUniformSet( set: 1 );

			if ( Source.indexCount == 0 )
				return;

			indices.Allocate( (uint)indicesList.Count, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
			indices.Upload( copy, indicesList.AsSpan() );
			vertices.Allocate( (uint)verticesList.Count, stagingHint: BufferUsage.None, deviceHint: BufferUsage.GpuRead | BufferUsage.GpuPerFrame );
			vertices.Upload( copy, verticesList.AsSpan() );
			uniforms.AllocateUniform( 1, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );

			uniformSet.SetUniformBuffer( uniforms, binding: 0 );
			uniformSet.SetSampler( texture.Value, binding: 1 );
		}

		public override void Draw ( ICommandBuffer commands ) {
			var shaders = shader.Value;
			ref var indices = ref Source.indices;
			ref var vertices = ref Source.vertices;
			ref var uniforms = ref Source.uniforms;
			ref var uniformSet = ref Source.uniformSet;

			var renderer = commands.Renderer;

			if ( textMeshUpload.Validate( ref Source.textMesh ) )
				updateTextMesh( renderer );
			if ( Source.indexCount == 0 )
				return;

			shaders.SetUniformSet( uniformSet!, set: 1 );

			commands.SetShaders( shaders );
			commands.BindVertexBuffer( vertices!.DeviceBuffer );
			commands.BindIndexBuffer( indices!.DeviceBuffer );
			uniforms!.UploadUniform( new Uniforms {
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

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
