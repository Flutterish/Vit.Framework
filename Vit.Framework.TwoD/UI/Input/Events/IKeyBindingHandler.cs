using Vit.Framework.Input.Events;

namespace Vit.Framework.TwoD.UI.Input.Events;

public abstract record KeyEvent<TKey> : UIEvent where TKey : struct, Enum {
	public required TKey Key { get; init; }
}

public record KeyDownEvent<TKey> : KeyEvent<TKey>, INonPropagableEvent where TKey : struct, Enum { } // TODO also create propagable keybinds
public record KeyUpEvent<TKey> : KeyEvent<TKey>, INonPropagableEvent where TKey : struct, Enum { }
public record KeyRepeatEvent<TKey> : KeyEvent<TKey>, INonPropagableEvent where TKey : struct, Enum { }

public interface IKeyBindingHandler<TKey> : IEventHandler<KeyDownEvent<TKey>>, IEventHandler<KeyUpEvent<TKey>>, IEventHandler<KeyRepeatEvent<TKey>> where TKey : struct, Enum {
	bool OnKeyDown ( TKey key, bool isRepeat );
	bool OnKeyUp ( TKey key );

	bool IEventHandler<KeyDownEvent<TKey>>.OnEvent ( KeyDownEvent<TKey> @event ) {
		return OnKeyDown( @event.Key, isRepeat: false );
	}
	bool IEventHandler<KeyRepeatEvent<TKey>>.OnEvent ( KeyRepeatEvent<TKey> @event ) {
		return OnKeyDown( @event.Key, isRepeat: true );
	}
	bool IEventHandler<KeyUpEvent<TKey>>.OnEvent ( KeyUpEvent<TKey> @event ) {
		return OnKeyUp( @event.Key );
	}
}
