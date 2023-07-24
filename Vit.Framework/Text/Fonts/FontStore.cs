namespace Vit.Framework.Text.Fonts;

public class FontStore {
	public static readonly FontIdentifier DefaultFont = new() { Name = "Default Font" };

	Dictionary<FontIdentifier, Font> fonts = new();
	public void AddFont ( FontIdentifier identifier, Font font ) {
		fonts.Add( identifier, font );
	}

	public Font GetFont ( FontIdentifier identifier ) {
		return fonts[identifier];
	}
}

public class FontIdentifier {
	public required string Name;
}