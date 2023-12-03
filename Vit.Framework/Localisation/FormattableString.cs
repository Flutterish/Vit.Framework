namespace Vit.Framework.Localisation;

public class FormattableString : LocalisableString {
	public LocalisableString Source;
	public object[] Data;

	public FormattableString ( LocalisableString source, object[] data ) {
		Source = source;
		Data = data;
	}

	public override string Localise ( LocalisationStore store ) { // TODO default formatting is not enough, we will also need case hints
		return string.Format( store.GetFormatProvider(), Source.Localise( store ), Data );
	}
}
