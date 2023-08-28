using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Vit.Framework.Text.Fonts;

public class FontStore {
	public static readonly FontIdentifier DefaultFont = new() { Name = "Default Font" };
	public static readonly FontCollectionIdentifier DefaultFontCollection = new( DefaultFont );

	Dictionary<FontIdentifier, Font> fonts = new();
	public void AddFont ( FontIdentifier identifier, Font font ) {
		fonts.Add( identifier, font );
	}

	public Font GetFont ( FontIdentifier identifier ) {
		return fonts[identifier];
	}

	public bool TryGetFont ( FontIdentifier identifier, [NotNullWhen( true )] out Font? font ) {
		return fonts.TryGetValue( identifier, out font );
	}

	Dictionary<FontCollectionIdentifier, FontCollection> collections = new();
	public FontCollection GetFontCollection ( FontCollectionIdentifier identifier ) {
		if ( !collections.TryGetValue( identifier, out var collection ) ) {
			collections.Add( identifier, collection = new FontCollection( identifier.Fonts.Select( GetFont ) ) );
		}

		return collection;
	}
}

public class FontIdentifier {
	public required string Name;
}

public class FontCollectionIdentifier {
	public readonly ImmutableArray<FontIdentifier> Fonts;
	public FontCollectionIdentifier ( params FontIdentifier[] fonts ) {
		Fonts = fonts.ToImmutableArray();
	}
}