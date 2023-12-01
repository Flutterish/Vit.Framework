using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Outlines;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using Vertex = Vit.Framework.TwoD.Rendering.Shaders.SvgVertex.Vertex;

namespace Vit.Framework.TwoD.Graphics.Text;

public class SpriteFontStore : IDrawDependency {
	public readonly Size2<uint> PageSize;
	public readonly Size2<uint> GlyphSize;
	public SpriteFontStore ( Size2<uint> pageSize, Size2<uint> glyphSize, ShaderStore shaders ) {
		PageSize = pageSize;
		GlyphSize = glyphSize;

		shader = shaders.GetShader( new() {
			Vertex = new() {
				Shader = SvgVertex.Identifier,
				Input = SvgVertex.InputDescription
			},
			Fragment = SvgFragment.Identifier
		} );
	}

	IRenderer renderer = null!;
	SingleUseBufferSectionStack singleUseBuffers = null!;
	public void Initialize ( IRenderer renderer, IReadOnlyDependencyCache dependencies ) {
		this.renderer = renderer;
		this.singleUseBuffers = dependencies.Resolve<SingleUseBufferSectionStack>();

		foreach ( var (_, i) in fonts ) {
			i.Initialize( renderer, shader.Value );
		}
	}

	Shader shader;
	Dictionary<Font, SpriteFont> fonts = new();
	public SpriteFont GetSpriteFont ( Font font ) {
		if ( !fonts.TryGetValue( font, out var spriteFont ) ) {
			fonts.Add( font, spriteFont = new( font, singleUseBuffers, PageSize, GlyphSize ) );
			spriteFont.Initialize( renderer, shader.Value );
		}

		return spriteFont;
	}

	public void Dispose () {
		foreach ( var (_, i) in fonts ) {
			i.Dispose();
		}
	}

	public void EndFrame () {
		
	}
}

public class SpriteFont : IDisposable {
	public readonly Font Font;
	public readonly Size2<uint> PageSize;
	public readonly Size2<uint> GlyphSize;
	public readonly uint GlyphsPerPage;
	public SpriteFont ( Font font, SingleUseBufferSectionStack singleUseBuffers, Size2<uint> pageSize, Size2<uint> glyphSize ) {
		PageSize = pageSize;
		GlyphSize = glyphSize;
		Font = font;
		SingleUseBuffers = singleUseBuffers;

		GlyphsPerPage = PageSize.Width * pageSize.Height;
	}

	public void Initialize ( IRenderer renderer, IShaderSet shaders ) {
		Shaders = shaders;
		Renderer = renderer;

		foreach ( var (index, page) in pages ) {
			page.Initialize( this, new( index * GlyphsPerPage ) );
		}
	}

	public readonly SingleUseBufferSectionStack SingleUseBuffers;
	public IShaderSet Shaders = null!;
	public IRenderer Renderer = null!;
	Dictionary<uint, SpriteFontPage> pages = new();
	public SpriteFontPage GetPage ( GlyphId glyph ) {
		var index = (uint)(glyph.Value / GlyphsPerPage);
		if ( !pages.TryGetValue( index, out var page ) ) {
			pages.Add( index, page = new( this, new( index * GlyphsPerPage ) ) );
			page.Initialize( this, new( index * GlyphsPerPage ) );
		}

		return page;
	}

	public void Dispose () {
		foreach ( var (_, i) in pages ) {
			i.Dispose();
		}
	}
}

public class SpriteFontPage : IDisposable { // TODO maybe we should also use a temp canvas (then downscale)?
	IDeviceTexture2D texture = null!;
	ITexture2DView view = null!;

	IDeviceTexture2D stencil = null!; // TODO these 2 should be disposed after drawing
	IFramebuffer canvas = null!;

	[ThreadStatic]
	static HashSet<GlyphId>? unresolvedGlyphs;

	public ITexture2DView View => view;

	AxisAlignedBox2<float>[] boundingBoxes;
	bool[] hasOwnColor;
	public AxisAlignedBox2<float> GetUvBox ( GlyphId glyph ) {
		return boundingBoxes[glyph.Value % (ulong)boundingBoxes.Length];
	}
	public bool HasOwnColor ( GlyphId glyph ) {
		return hasOwnColor[glyph.Value % (ulong)boundingBoxes.Length];
	}

	public SpriteFontPage ( SpriteFont font, GlyphId firstGlyph ) {
		boundingBoxes = new AxisAlignedBox2<float>[font.PageSize.Width * font.PageSize.Height];
		hasOwnColor = new bool[font.PageSize.Width * font.PageSize.Height];
	}

	public void Initialize ( SpriteFont font, GlyphId firstGlyph ) { // TODO some method of logging
		Console.WriteLine( $"Generating {font.PageSize.Width * font.PageSize.Height} glyphs for sprite font {font.Font.Name} ({firstGlyph.Value}-{firstGlyph.Value+ font.PageSize.Width * font.PageSize.Height})" );
		generate( font, firstGlyph );
	}

	unsafe void generate ( SpriteFont font, GlyphId firstGlyph ) {
		var renderer = font.Renderer;
		var uvMatrix = new Matrix4x3<float>( renderer.CreateUvCorrectionMatrix<float>() );
		var pageSize = font.PageSize;
		var glyphSize = font.GlyphSize;

		var size = new Size2<uint>( pageSize.Width * glyphSize.Width, pageSize.Height * glyphSize.Height ); // in theory we can optimize this based on present glyphs instead of predefined per-glyph sizes
		var glyphCount = pageSize.Width * pageSize.Height;

		texture = renderer.CreateDeviceTexture( size, PixelFormat.Rgba8 );
		stencil = renderer.CreateDeviceTexture( size, PixelFormat.S8ui );
		view = texture.CreateView();

		canvas = renderer.CreateFramebuffer( new[] { texture }, stencil );

		AxisAlignedBox2<float> getBounds ( GlyphId id ) {
			var offset = (uint)(id.Value - firstGlyph.Value);
			return new() {
				MinX = ((offset % pageSize.Width) * glyphSize.Width + 1) / (float)size.Width * 2 - 1,
				MaxX = ((offset % pageSize.Width + 1) * glyphSize.Width - 1) / (float)size.Width * 2 - 1,
				MinY = ((offset / pageSize.Width) * glyphSize.Height + 1) / (float)size.Height * 2 - 1,
				MaxY = ((offset / pageSize.Width + 1) * glyphSize.Height - 1) / (float)size.Height * 2 - 1
			};
		}

		using var commands = renderer.CreateImmediateCommandBuffer();
		commands.SetShaders( font.Shaders );

		unresolvedGlyphs ??= new( (int)glyphCount );
		foreach ( var i in Enumerable.Range( 0, (int)glyphCount ) ) {
			unresolvedGlyphs.Add( new( firstGlyph.Value + (uint)i ) );
		}

		using var _ = commands.RenderTo( canvas );
		commands.SetTopology( Topology.Triangles );
		commands.SetViewport( size );
		commands.SetScissors( size );
		commands.ClearColor( ColorSRgba.Transparent );
		commands.SetDepthTest( new() { IsEnabled = false }, new() { WriteOnPass = false } );

		void renderOutline ( IEnumerable<Spline2<double>> outline, AxisAlignedBox2<double> glyphBounds, AxisAlignedBox2<float> bounds, ColorSRgba<float> color ) {
			var stencil = new StencilGlyph( outline ); // TODO generate this on the fly

			var indices = font.SingleUseBuffers.AllocateHostBuffer<uint>( (uint)stencil.Indices.Count, BufferType.Index );
			var vertices = font.SingleUseBuffers.AllocateHostBuffer<Vertex>( (uint)stencil.Vertices.Count, BufferType.Vertex );
			var vertexPtr = vertices.Map();

			foreach ( var i in stencil.Vertices ) {
				var pos = i.Cast<double>() - glyphBounds.Position;
				pos.X = pos.X * bounds.Width / glyphBounds.Width;
				pos.Y = pos.Y * bounds.Height / glyphBounds.Height;
				*vertexPtr = new() {
					Color = color,
					Position = uvMatrix.Apply( bounds.Position.Cast<float>() + pos.Cast<float>() )
				};
				vertexPtr++;
			}

			vertices.Unmap();
			indices.Upload( stencil.Indices.AsSpan() );

			commands.BindIndexBufferRaw( indices.Buffer, IndexBufferType.UInt32, offset: indices.Offset );
			commands.BindVertexBufferRaw( vertices.Buffer, offset: vertices.Offset );
			using ( commands.PushStencilTest( new( CompareOperation.Never ), new() { CompareMask = 1, WriteMask = 1, StencilFailOperation = StencilOperation.Invert } ) ) {
				commands.DrawIndexed( (uint)stencil.Indices.Count );

				commands.SetStencilTest( new( CompareOperation.Equal ), new() { CompareMask = 1, WriteMask = 1, ReferenceValue = 1, PassOperation = StencilOperation.SetTo0 } );
				commands.DrawIndexed( (uint)stencil.Indices.Count );
			}
		}

		foreach ( var (id, outline) in font.Font.FetchOutlines<SvgOutline>( unresolvedGlyphs ) ) {
			unresolvedGlyphs.Remove( id ); // TODO fillrule
			var bounds = getBounds( id );

			var glyphBounds = outline.CalculateBoundingBox();
			hasOwnColor[id.Value - firstGlyph.Value] = true;
			boundingBoxes[id.Value - firstGlyph.Value] = new() {
				MinX = (bounds.MinX + 1) / 2,
				MinY = (bounds.MinY + 1) / 2,
				MaxX = (bounds.MaxX + 1) / 2,
				MaxY = (bounds.MaxY + 1) / 2
			};

			foreach ( var element in outline.Elements ) {
				renderOutline( element.Splines, glyphBounds, bounds, element.Fill?.ToFloat<float>().ToSRgb() ?? ColorSRgba.White );
			}
		}

		if ( !unresolvedGlyphs.Any() )
			return;

		foreach ( var (id, outline) in font.Font.FetchOutlines<SplineOutline>( unresolvedGlyphs ) ) { // 🍆 😊
			unresolvedGlyphs.Remove( id );
			var bounds = getBounds( id );

			var glyphBounds = outline.CalculateBoundingBox();
			boundingBoxes[id.Value - firstGlyph.Value] = new() {
				MinX = (bounds.MinX + 1) / 2,
				MinY = (bounds.MinY + 1) / 2,
				MaxX = (bounds.MaxX + 1) / 2,
				MaxY = (bounds.MaxY + 1) / 2
			};

			renderOutline( outline.Splines, glyphBounds, bounds, ColorSRgba.White );
		}

		unresolvedGlyphs.Clear();
	}

	public void Dispose () {
		canvas.Dispose();
		stencil.Dispose();
		view.Dispose();
		texture.Dispose();
	}
}