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

public class EventTree<TSelf> where TSelf : class, IHasEventTrees<TSelf> { // TODO I would really like these to be strongly typed (event type)
	public required TSelf Source { get; init; }

	public int Depth { get; private set; }
	public Func<Event, bool>? Handler;
	List<EventTree<TSelf>>? children;
	public EventTree<TSelf>? Parent { get; private set; }
	public IReadOnlyList<EventTree<TSelf>> Children => children ?? (IReadOnlyList<EventTree<TSelf>>)Array.Empty<EventTree<TSelf>>();
	
	public void Add ( EventTree<TSelf> child ) {
		(children ??= new()).Add( child );
		child.Depth = children.Count - 1;
		child.Parent = this;
	}
	public void Remove ( EventTree<TSelf> child ) {
		var index = children!.IndexOf( child );
		children!.RemoveAt( index );
		child.Parent = null;
		for ( int i = index; i < children.Count; i++ ) {
			children[i].Depth--;
		}
	}

	public void Sort ( Comparison<EventTree<TSelf>> comparer ) {
		children!.Sort( comparer );
		for ( int i = 0; i < children.Count; i++ ) {
			children[i].Depth = i;
		}
	}

	public EventTree<TSelf>? Next {
		get {
			if ( Children.Any() == true )
				return Children[0];

			var node = this;
			var parent = Parent;
			while ( parent != null ) {
				if ( parent.Children.Count > node.Depth + 1 )
					return parent.Children[node.Depth + 1];

				node = parent;
				parent = node.Parent;
			}

			return null;
		}
	}

	public EventTree<TSelf>? NextWithHandler {
		get {
			var node = Next;
			while ( node != null ) {
				if ( node.Handler != null )
					return node;

				node = node.Next;
			}

			return null;
		}
	}

	public EventTree<TSelf>? Previous {
		get {
			var parent = Parent;
			if ( parent == null )
				return null;

			if ( Depth == 0 )
				return parent;

			var sibling = parent.Children[Depth - 1];
			while ( sibling != null && sibling.Children.Any() )
				sibling = sibling.Children[^1];

			return sibling;
		}
	}

	public EventTree<TSelf>? PreviousWithHandler {
		get {
			var node = Previous;
			while ( node != null ) {
				if ( node.Handler != null )
					return node;

				node = node.Previous;
			}

			return null;
		}
	}

	public bool ShouldBeCulled => Handler == null && Children.Any() != true;

	[ThreadStatic]
	static Stack<EventTree<TSelf>>? enumerationStack;
	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateHandlers () {
		var stack = enumerationStack ??= new();

		try {
			stack.Push( this );

			while ( stack.TryPop( out var node ) ) {
				if ( node.Handler != null )
					yield return (node.Source, node.Handler);

				if ( node.children == null )
					continue;

				foreach ( var i in node.children ) {
					stack.Push( i );
				}
			}
		}
		finally {
			enumerationStack.Clear();
		}
	}

	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateCulledHandlers ( Func<TSelf, bool> predicate ) {
		var stack = enumerationStack ??= new();
		
		try {
			stack.Push( this );

			while ( stack.TryPop( out var node ) ) {
				if ( !predicate( node.Source ) )
					continue;

				if ( node.Handler != null )
					yield return (node.Source, node.Handler);

				if ( node.children == null )
					continue;

				foreach ( var i in node.children ) {
					stack.Push( i );
				}
			}
		}
		finally {
			enumerationStack.Clear();
		}
	}

	public IEnumerable<(TSelf, Func<Event, bool>)> EnumerateCulledHandlers<TData> ( TData data, Func<TSelf, TData, bool> predicate ) {
		var stack = enumerationStack ??= new();
		
		try {
			stack.Push( this );

			while ( stack.TryPop( out var node ) ) {
				if ( !predicate( node.Source, data ) )
					continue;

				if ( node.Handler != null )
					yield return (node.Source, node.Handler);

				if ( node.children == null )
					continue;

				foreach ( var i in node.children ) {
					stack.Push( i );
				}
			}
		}
		finally {
			enumerationStack.Clear();
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