namespace Vit.Framework.Input;

public class BinaryStateTracker<T> {
	HashSet<T> previous = new();
	HashSet<T> current = new();

	public void Add ( T value ) {
		current.Add( value );
	}

	public void Remove ( T value ) {
		current.Remove( value );
	}

	public void Tick () {
		(previous, current) = (current, previous);
		current.Clear();
	}

	public bool IsActive ( T value )
		=> current.Contains( value );

	public bool IsInactive ( T value )
		=> !current.Contains( value );

	public bool WasActivated ( T value )
		=> current.Contains( value ) && !previous.Contains( value );

	public bool WasDeactivated ( T value )
		=> !current.Contains( value ) && previous.Contains( value );

	public bool WasActive ( T value )
		=> previous.Contains( value );

	public bool WasInactive ( T value )
		=> !previous.Contains( value );
}
