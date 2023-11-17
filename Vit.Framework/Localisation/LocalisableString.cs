namespace Vit.Framework.Localisation;

public struct LocalisableString {
	public required LocalisableStringData Data;

	[Obsolete( "Raw string was cast to a localisable string. Consider making this data localisable." )]
	public static implicit operator LocalisableString ( string data ) {
		return new() {
			Data = new RawString( data )
		};
	}

	public static implicit operator LocalisableString ( LocalisableStringData data )
		=> new() { Data = data };

	public override string ToString () {
		return Data?.ToString() ?? string.Empty;
	}
}

public abstract class LocalisableStringData {
	public abstract string Localise ( LocalisationStore store );

	static readonly LocalisationStore emptyStore = new();
	public override string ToString () {
		return Localise( emptyStore );
	}
}
