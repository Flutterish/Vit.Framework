using System.Reflection;

namespace Vit.Framework.Input.Events;

public interface IHasEventTrees<TSelf> where TSelf : class, IHasEventTrees<TSelf> {
	IReadOnlyDictionary<Type, EventTree<TSelf>> HandledEventTypes { get; }

	event Action<Type, EventTree<TSelf>>? EventHandlerAdded;
	event Action<Type, EventTree<TSelf>>? EventHandlerRemoved;

	[ThreadStatic]
	private static Dictionary<Type, Action<TSelf>>? typeInitializer;
	[ThreadStatic]
	private static object[]? initializerParams;
	private static void addHandler<TEvent> ( TSelf self, Action<TSelf, Type, Func<Event, bool>> adder ) where TEvent : Event {
		adder( self, typeof(TEvent), e => ((IEventHandler<TEvent>)self).OnEvent( (TEvent)e ) );
	}
	private static MethodInfo _adder = typeof( IHasEventTrees<TSelf> ).GetMethod( nameof( addHandler ), BindingFlags.Static | BindingFlags.NonPublic )!;

	/// <summary>
	/// Adds event handlers declared by <see cref="IEventHandler{TEvent}"/> interfaces.
	/// </summary>
	public static void AddDeclaredEventHandlers ( TSelf self, Action<TSelf, Type, Func<Event, bool>> adder ) {
		var type = self.GetType();
		if ( !(typeInitializer ??= new()).TryGetValue( type, out var action ) ) {
			var interfaces = type.GetInterfaces().Where( x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof( IEventHandler<> ) );
			var eventTypes = interfaces.Select( x => x.GenericTypeArguments[0] );

			var adders = eventTypes.Select( x => _adder.MakeGenericMethod( x ) ).ToArray();

			typeInitializer.Add( type, action = self => { // TODO this should probably be source generated
				initializerParams ??= new object[2];
				initializerParams[0] = self;
				initializerParams[1] = adder;
				foreach ( var i in adders ) {
					i.Invoke( null, initializerParams );
				}
			} );
		}

		action( self );
	}
}

public static class IHasEventTreesExtensions {
	public static bool TriggerEventOnSelf<TSelf> ( this IHasEventTrees<TSelf> self, Event @event ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		while ( type != null ) { // TODO the base types should be checked simultaniously
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.Handler is Func<Event, bool> handler && handler( @event ) )
				return true;

			type = type.BaseType;
		}

		return false;
	}

	public static TSelf? TriggerEvent<TSelf> ( this IHasEventTrees<TSelf> self, Event @event ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		while ( type != null ) {
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerEvent( @event ) is TSelf handler )
				return handler;

			type = type.BaseType;
		}

		return null;
	}

	public static TSelf? TriggerCulledEvent<TSelf> ( this IHasEventTrees<TSelf> self, Event @event, Func<TSelf, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		while ( type != null ) {
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerCulledEvent( @event, predicate ) is TSelf handler )
				return handler;

			type = type.BaseType;
		}

		return null;
	}

	public static TSelf? TriggerCulledEvent<TSelf, TData> ( this IHasEventTrees<TSelf> self, Event @event, TData data, Func<TSelf, TData, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		while ( type != null ) {
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerCulledEvent( @event, data, predicate ) is TSelf handler )
				return handler;

			type = type.BaseType;
		}

		return null;
	}
}

public class EventTree<TSelf> where TSelf : class, IHasEventTrees<TSelf> {
	public required TSelf Source { get; init; }

	public Func<Event, bool>? Handler;
	public List<EventTree<TSelf>>? Children;

	public bool ShouldBeCulled => Handler == null && Children?.Any() != true;

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

	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateCulledHandlers ( Func<TSelf, bool> predicate ) {
		var stack = enumerationStack ??= new();
		stack.Push( this );

		while ( stack.TryPop( out var node ) ) {
			if ( !predicate( node.Source ) )
				continue;

			if ( node.Handler != null )
				yield return (node.Source, node.Handler);

			if ( node.Children == null )
				continue;

			foreach ( var i in node.Children ) {
				stack.Push( i );
			}
		}
	}

	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateCulledHandlers<TData> ( TData data, Func<TSelf, TData, bool> predicate ) {
		var stack = enumerationStack ??= new();
		stack.Push( this );

		while ( stack.TryPop( out var node ) ) {
			if ( !predicate( node.Source, data ) )
				continue;

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

	public TSelf? TriggerCulledEvent ( Event @event, Func<TSelf, bool> predicate ) {
		foreach ( var (i, handler) in EnumerateCulledHandlers( predicate ) ) {
			if ( handler( @event ) )
				return i;
		}

		return null;
	}

	public TSelf? TriggerCulledEvent<TData> ( Event @event, TData data, Func<TSelf, TData, bool> predicate ) {
		foreach ( var (i, handler) in EnumerateCulledHandlers( data, predicate ) ) {
			if ( handler( @event ) )
				return i;
		}

		return null;
	}
}