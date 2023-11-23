using Vit.Framework.Graphics;
using Vit.Framework.Text.Fonts;

namespace Vit.Framework.TwoD.Graphics;

public static class FrameworkUIScheme {
	public static readonly ColorRgb<float> Element = ColorRgb.GreenYellow;
	public static readonly ColorRgb<float> ElementPressed = ColorRgb.YellowGreen.Interpolate( ColorRgb.Black, 0.1f );
	public static readonly ColorRgb<float> ElementHovered = Element.Interpolate( ElementPressed, 0.5f );
	public static readonly ColorRgb<float> ElementFlash = ColorRgb.White;

	public static readonly FontIdentifier Font = new() { Name = "Framework Default (Consolas)" };
	public static readonly FontIdentifier EmojiFont = new() { Name = "Framework Default Emojis (Twemoji)" };
	public static readonly FontCollectionIdentifier FontCollection = new( Font, EmojiFont );
}
