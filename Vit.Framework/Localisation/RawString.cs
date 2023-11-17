namespace Vit.Framework.Localisation;

public class RawString : LocalisableStringData {
	public string Data;

	public RawString ( string data ) {
		Data = data;
	}

	public override string Localise ( LocalisationStore store ) {
		return Data;
	}
}
