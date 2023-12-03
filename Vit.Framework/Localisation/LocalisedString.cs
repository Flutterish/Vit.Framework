namespace Vit.Framework.Localisation;

public partial class LocalisationStore {
	public class LocalisedString {
		string value = string.Empty;
		public string Value {
			get => value;
			private set {
				if ( value.TrySet( ref this.value ) )
					ValueChanged?.Invoke( value );
			}
		}

		LocalisationStore? store;
		/// <summary>
		/// Sets the store to get localisation data from. This puts it in an internal linked list which listens for updates.
		/// Set this to <see langword="null"/> to unsubscribe it from the localisation store updates.
		/// </summary>
		public LocalisationStore? Store {
			get => store;
			set {
				if ( store != null ) {
					if ( store.last == this ) {
						store.last = previous;
					}
					if ( store.first == this ) {
						store.first = next;
					}
					if (previous != null) {
						previous.next = next;
					}
					if (next != null) {
						next.previous = previous;
					}
					next = null;
					previous = null;
				}
				store = value;
				if ( store != null ) {
					if ( store.last == null ) {
						store.first = this;
						store.last = this;
					}
					else {
						previous = store.last;
						store.last.next = this;
						store.last = this;
					}
				}

				Update();
			}
		}

		public void SetRaw ( string raw ) {
			Value = raw;
			localisable = null;
			ValueChanged?.Invoke( Value );
		}

		LocalisableString? localisable = null;
		public LocalisableString? LocalisableString {
			get => localisable;
			set {
				localisable = value;
				Update();
			}
		}

		public void Update () {
			if ( localisable == null || store == null )
				return;

			Value = localisable.Localise( store );
		}

		public event Action<string>? ValueChanged;
		LocalisedString? previous;
		LocalisedString? next;

		internal static void updateAll ( LocalisedString? node ) {
			while ( node != null ) {
				node.Update();
				node = node.next;
			}
		}
	}

	LocalisedString? first;
	LocalisedString? last;
}
