using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.Text;
using Vit.Framework.Parsing;
using Vit.Framework.Text.Fonts.OpenType;

namespace Vit.Framework.Tests.Layout;

public class SpriteTextTest : LayoutContainer<ILayoutElement> {
	OpenTypeFont font;
	public SpriteTextTest () {
		font = new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) );

		AddChild( new Sprite { Tint = ColorRgba.Green }, new() {
			Size = new( 1f.Relative(), 64 ),
			Anchor = new( 0, 100 )
		} );
		AddChild( new SpriteText {
			Text = "Hello, World!",
			FontSize = 64,
			Font = font,
			Tint = ColorRgba.Black
		}, new() {
			Anchor = new( 0, 100 )
		} );
	}
}
