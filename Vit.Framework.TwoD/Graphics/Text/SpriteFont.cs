using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.TwoD.Graphics.Text;

public class SpriteFontStore : DisposableObject {
	public readonly Size2<uint> PageSize;
	public readonly Size2<uint> GlyphSize;
	public SpriteFontStore ( Size2<uint> pageSize, Size2<uint> glyphSize ) {
		PageSize = pageSize;
		GlyphSize = glyphSize;
	}

	Dictionary<Font, SpriteFont> fonts = new();
	public SpriteFont GetSpriteFont ( Font font ) {
		if ( !fonts.TryGetValue( font, out var spriteFont ) ) {
			fonts.Add( font, spriteFont = new( font, PageSize, GlyphSize ) );
		}

		return spriteFont;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in fonts ) {
			i.Dispose();
		}
	}
}

public class SpriteFont : DisposableObject {
	public readonly Font Font;
	public readonly Size2<uint> PageSize;
	public readonly Size2<uint> GlyphSize;
	public readonly uint GlyphsPerPage;
	public SpriteFont ( Font font, Size2<uint> pageSize, Size2<uint> glyphSize ) {
		PageSize = pageSize;
		GlyphSize = glyphSize;
		Font = font;

		GlyphsPerPage = PageSize.Width * pageSize.Height;
	}

	Dictionary<uint, SpriteFontAtlas> pages = new();
	public SpriteFontAtlas GetPage ( GlyphId glyph ) {
		var index = (uint)(glyph.Value / GlyphsPerPage);
		if ( !pages.TryGetValue( index, out var page ) )
			pages.Add( index, page = new() );

		return page;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in pages ) {
			i.Dispose();
		}
	}
}

public class SpriteFontAtlas : DisposableObject { // TODO maybe we should also use a temp canvas?


	protected override void Dispose ( bool disposing ) {
		throw new NotImplementedException();
	}
}