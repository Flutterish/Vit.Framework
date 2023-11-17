namespace Vit.Framework.Localisation;

public class LocalisedString {
	string text = string.Empty;
	public string Text {
		get => text;
		set {
			text = value;
			TextChanged?.Invoke( value );
		}
	}

	public void BindTextChanged ( Action<string> handler ) {
		TextChanged += handler;
		handler( text );
	}
	public event Action<string>? TextChanged;

	public override string ToString () {
		return text;
	}
}
