using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Input.Events.EventSources;

public class KeyBindingEventSource<TKey, THandler> where TKey : struct, Enum where THandler : class, IHasEventTrees<THandler> {
	public required THandler Root { get; init; }
	Dictionary<TKey, THandler> pressHandlers = new();

	public bool Press ( TKey key, Millis timestamp, THandler target ) {
		Release( key, timestamp );

		var handler = target.TriggerEvent( new KeyDownEvent<TKey> { Key = key, Timestamp = timestamp } );
		if ( handler == null )
			return false;

		pressHandlers[key] = handler;
		return true;
	}

	public bool Press ( TKey key, Millis timestamp ) {
		Release( key, timestamp );

		var handler = Root.TriggerEvent( new GlobalKeyDownEvent<TKey> { Key = key, Timestamp = timestamp } );
		if ( handler == null )
			return false;

		pressHandlers[key] = handler;
		return true;
	}

	public bool Repeat ( TKey key, Millis timestamp ) {
		if ( !pressHandlers.TryGetValue( key, out var handler ) )
			return Press( key, timestamp );

		if ( !handler.TriggerEventOnSelf( new KeyRepeatEvent<TKey> { Key = key, Timestamp = timestamp } ) )
			return Press( key, timestamp );
		return true;
	}

	public bool Release ( TKey key, Millis timestamp ) {
		if ( !pressHandlers.Remove( key, out var handler ) )
			return false;

		return handler.TriggerEventOnSelf( new KeyUpEvent<TKey> { Key = key, Timestamp = timestamp } );
	}
}
