namespace Vit.Framework.Input.Events;

public interface IEventHandler<TSelf> where TSelf : class, IEventHandler<TSelf> {
	IReadOnlyDictionary<Type, EventTree<TSelf>> HandledEventTypes { get; }

	event Action<Type, EventTree<TSelf>>? EventHandlerAdded;
	event Action<Type, EventTree<TSelf>>? EventHandlerRemoved;
}

public static class IEventHandlerExtensions {
	public static TSelf? TriggerEvent<TSelf> ( this IEventHandler<TSelf> self, Event @event ) where TSelf : class, IEventHandler<TSelf> {
		var type = @event.GetType();
		while ( type != null ) {
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerEvent( @event ) is TSelf handler )
				return handler;

			type = type.BaseType;
		}

		return null;
	}
}

public class EventTree<TSelf> where TSelf : class, IEventHandler<TSelf> {
	public required TSelf Source { get; init; }

	public Func<Event, bool>? Handler;
	public List<EventTree<TSelf>>? Children;

	[ThreadStatic]
	static Stack<EventTree<TSelf>>? enumerationStack;
	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateHandlers () {
		var stack = enumerationStack ??= new();
		stack.Push( this );

		while ( stack.TryPop( out var node ) ) {
			if ( node.Handler != null )
				yield return (node.Source, node.Handler);

			if ( node.Children == null )
				continue;

			foreach ( var i in node.Children ) {
				stack.Push( i );
			}
		}
	}

	public TSelf? TriggerEvent ( Event @event ) {
		foreach ( var (i, handler) in EnumerateHandlers() ) {
			if ( handler( @event ) )
				return i;
		}

		return null;
	}
}