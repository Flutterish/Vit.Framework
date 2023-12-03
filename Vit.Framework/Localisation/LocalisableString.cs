namespace Vit.Framework.Localisation;

public abstract class LocalisableString {
	public abstract string Localise ( LocalisationStore store );

	static readonly LocalisationStore emptyStore = new();
	public override string ToString () {
		return Localise( emptyStore );
	}

	[Obsolete( "Raw string was cast to a localisable string. Consider making this data localisable." )]
	public static implicit operator LocalisableString ( string data ) {
		return new RawString( data );
	}
}
