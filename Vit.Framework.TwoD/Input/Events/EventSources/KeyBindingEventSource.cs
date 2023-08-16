using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.Input.Events.EventSources;

public class KeyBindingEventSource<TKey, THandler> where TKey : struct, Enum where THandler : class, IHasEventTrees<THandler> {
	public required THandler Root { get; init; }
	Dictionary<TKey, THandler> pressHandlers = new();

	public bool Press ( TKey key, THandler target ) {
		Release( key );

		var handler = target.TriggerEvent( new KeyDownEvent<TKey> { Key = key } );
		if ( handler == null )
			return false;

		pressHandlers[key] = handler;
		return true;
	}

	public bool Press ( TKey key ) {
		Release( key );

		var handler = Root.TriggerEvent( new KeyDownEvent<TKey> { Key = key } );
		if ( handler == null )
			return false;

		pressHandlers[key] = handler;
		return true;
	}

	public bool Repeat ( TKey key ) {
		if ( !pressHandlers.TryGetValue( key, out var handler ) )
			return Press( key );

		if ( !handler.TriggerEventOnSelf( new KeyRepeatEvent<TKey> { Key = key } ) )
			return Press( key );
		return true;
	}

	public bool Release ( TKey key ) {
		if ( !pressHandlers.Remove( key, out var handler ) )
			return false;

		return handler.TriggerEventOnSelf( new KeyUpEvent<TKey> { Key = key } );
	}
}
