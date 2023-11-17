using System.Reflection;

namespace Vit.Framework.Localisation;

public class TranslatableString : LocalisableStringData {
	public string Fallback;

	public Assembly Assembly;
	public string Key;

	public TranslatableString ( Assembly assembly, string key, string fallback ) {
		Assembly = assembly;
		Key = key;
		Fallback = fallback;
	}

	public override string Localise ( LocalisationStore store ) {
		return store.Lookup( Assembly, Key ) ?? Fallback;
	}
}
