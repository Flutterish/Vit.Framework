using Vit.Framework.Graphics;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.TwoD.Graphics;

public static class FrameworkUIScheme {
	public static readonly ColorRgba<float> Element = ColorRgba.GreenYellow;
	public static readonly ColorRgba<float> ElementPressed = ColorRgba.YellowGreen.Interpolate( ColorRgba.Black, 0.1f );
	public static readonly ColorRgba<float> ElementHovered = Element.Interpolate( ElementPressed, 0.5f );
	public static readonly ColorRgba<float> ElementFlash = ColorRgba.White;

	public static readonly FontIdentifier Font = new() { Name = "Framework Default (Consolas)" };
	public static readonly FontIdentifier EmojiFont = new() { Name = "Framework Default Emojis (Twemoji)" };
	public static readonly FontCollectionIdentifier FontCollection = new( Font, EmojiFont );
}
