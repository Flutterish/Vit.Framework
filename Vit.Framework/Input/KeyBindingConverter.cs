using Vit.Framework.Collections;

namespace Vit.Framework.Input;

public class KeyBindingConverter<TFrom, TTo> where TFrom : struct, Enum where TTo : struct, Enum {
	List<KeyBinding<TFrom, TTo>> bindings = new();
	EnumerablePrefixTree<TFrom, List<TTo>> prefixTree = new();
	public IEnumerable<KeyBinding<TFrom, TTo>> Bindings {
		get => bindings;
		set {
			bindings.Clear();
			prefixTree.Clear();
			bindings.AddRange( value );
			foreach ( var i in value ) {
				var node = prefixTree.GetOrCreateNode( i.Binding );
				if ( !node.HasValue )
					node.SetValue( new() );
				node.Value.Add( i.Value );
			}
		}
	}


	HashSet<TTo> swapPressed = new();
	HashSet<TTo> pressed = new();

	HashSet<TFrom> unusedInputs = new();
	HashSet<TFrom> pressedInputs = new();

	public IEnumerable<TTo> PressedBindings => pressed;
	public IEnumerable<TFrom> PressedInputs => pressedInputs;
	public IEnumerable<TFrom> UnusedInputs => unusedInputs;

	public void Add ( TFrom value ) {
		if ( !pressedInputs.Add( value ) )
			return;

		computeChanges();
	}

	public void Remove ( TFrom value ) {
		if ( !pressedInputs.Remove( value ) )
			return;

		computeChanges();
	}

	public void Repeat ( TFrom value ) {
		Add( value );

		foreach ( var i in bindings.Where( x => x.Binding.Contains( value ) ) ) {
			if ( !pressed.Contains( i.Value ) )
				continue;

			Repeated?.Invoke( i.Value );
			return;
		}
	}

	public void MutateRange ( IEnumerable<TFrom>? add, IEnumerable<TFrom>? remove ) {
		bool anyChanged = false;
		if ( remove != null ) {
			foreach ( var i in remove ) {
				anyChanged |= pressedInputs.Remove( i );
			}
		}
		if ( add != null ) {
			foreach ( var i in add ) {
				anyChanged |= pressedInputs.Add( i );
			}
		}

		if ( anyChanged )
			computeChanges();
	}

	protected virtual IEnumerable<TFrom> CreatePersistent ( IEnumerable<TFrom> all ) {
		yield break;
	}

	enum ConsumedMode {
		PersistentOnly,
		Consumed,
		Failed
	}

	HashSet<TFrom> persistent = new();
	void computeChanges () { // TODO currently when multiple bindings to the same thing are activated it only activates once. we could count them instead
		persistent.Clear();
		foreach ( var i in CreatePersistent( pressedInputs ) )
			persistent.Add( i );

		unusedInputs.Clear();
		foreach ( var i in pressedInputs )
			unusedInputs.Add( i );

		ConsumedMode tryConsume ( PrefixTree<IEnumerable<TFrom>, TFrom, List<TTo>> node, bool anyConsumed ) {
			foreach ( var (key, child) in node.Children.OrderBy( x => x.Key ) ) {
				if ( !unusedInputs.Remove( key ) )
					continue;
				bool isPersistent = persistent.Contains( key );

				var result = tryConsume( child, anyConsumed || !isPersistent );
				if ( result == ConsumedMode.Consumed ) {
					if ( isPersistent )
						unusedInputs.Add( key );

					return ConsumedMode.Consumed;
				}
				else if ( result == ConsumedMode.PersistentOnly ) {
					if ( isPersistent )
						unusedInputs.Add( key );
				}
				else {
					unusedInputs.Add( key );
				}
			}

			if ( node.HasValue ) {
				foreach ( var i in node.Value ) {
					swapPressed.Add( i );
				}
				
				return anyConsumed ? ConsumedMode.Consumed : ConsumedMode.PersistentOnly;
			}
			else {
				return ConsumedMode.Failed;
			}
		}

		tryConsume( prefixTree, anyConsumed: false );

		foreach ( var i in pressed ) {
			if ( !swapPressed.Contains( i ) )
				Released?.Invoke( i );
		}

		foreach ( var i in swapPressed ) {
			if ( !pressed.Contains( i ) )
				Pressed?.Invoke( i );
		}

		(pressed, swapPressed) = (swapPressed, pressed);
		swapPressed.Clear();
	}

	public event Action<TTo>? Pressed;
	public event Action<TTo>? Repeated;
	public event Action<TTo>? Released;
}
