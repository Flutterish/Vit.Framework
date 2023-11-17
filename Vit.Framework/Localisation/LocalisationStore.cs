using System.Globalization;
using System.Reflection;

namespace Vit.Framework.Localisation;

public class LocalisationStore {
	Dictionary<LanguageIdentifier, LanguageStore> stores = new();
	public void Add ( LanguageIdentifier identifier, LanguageStore store ) {
		stores.Add( identifier, store );
	}

	LanguageStore language = EmptyInvariantLanguageStore.Instance;
	public void SetLanguage ( LanguageIdentifier identifier ) {
		language = stores[identifier];
		foreach ( var (data, result) in localisedStrings ) {
			updateString( data, result );
		}
	}

	public LocalisedString GetLocalised ( LocalisableString data ) {
		return GetLocalised( data.Data );
	}
	public LocalisedString GetLocalised ( LocalisableStringData data ) {
		if ( !localisedStrings.TryGetValue( data, out var localised ) ) {
			localisedStrings.Add( data, localised = new() );
			updateString( data, localised );
		}

		return localised;
	}

	public void UpdateString ( LocalisableString data ) {
		UpdateString( data.Data );
	}
	public void UpdateString ( LocalisableStringData data ) {
		updateString( data, localisedStrings[data] );
	}
	void updateString ( LocalisableStringData data, LocalisedString result ) {
		result.Text = data.Localise( this );
	}

	public IFormatProvider GetFormatProvider () {
		return language.GetFormatProvider();
	}
	public string? Lookup ( Assembly assembly, string key ) {
		return language.Lookup( assembly, key );
	}

	Dictionary<LocalisableStringData, LocalisedString> localisedStrings = new(); // TODO we dont need to keep track of strings that are no longer in use
}

public class LanguageIdentifier {
	public required string Name;

	static Dictionary<CultureInfo, LanguageIdentifier> forCultures = new();
	public static LanguageIdentifier ForCulture ( CultureInfo culture ) {
		lock ( forCultures ) {
			if ( !forCultures.TryGetValue( culture, out var id ) )
				forCultures.Add( culture, id = new() { Name = culture.Name } );

			return id;
		}
	}
}

public abstract class LanguageStore {
	public abstract IFormatProvider GetFormatProvider ();
	public abstract string? Lookup ( Assembly assembly, string key );
}

public sealed class EmptyInvariantLanguageStore : LanguageStore {
	private EmptyInvariantLanguageStore () { }
	public static readonly EmptyInvariantLanguageStore Instance = new();

	public override IFormatProvider GetFormatProvider () {
		return CultureInfo.InvariantCulture;
	}

	public override string? Lookup ( Assembly assembly, string key ) {
		return null;
	}
}