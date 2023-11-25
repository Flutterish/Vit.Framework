using System.Reflection;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
		return self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.Handler is Func<Event, bool> handler && handler( @event );
	}

	public static TSelf? TriggerEvent<TSelf> ( this IHasEventTrees<TSelf> self, Event @event ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerEvent( @event ) is TSelf handler )
			return handler;

		return null;
	}

	public static TSelf? TriggerCulledEvent<TSelf> ( this IHasEventTrees<TSelf> self, Event @event, Func<TSelf, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerCulledEvent( @event, predicate ) is TSelf handler )
			return handler;

		return null;
	}

	public static TSelf? TriggerCulledEvent<TSelf, TData> ( this IHasEventTrees<TSelf> self, Event @event, TData data, Func<TSelf, TData, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerCulledEvent( @event, data, predicate ) is TSelf handler )
			return handler;

		return null;
	}
}

public class EventTree<TTarget> where TTarget : class, IHasEventTrees<TTarget> { // TODO I would really like these to be strongly typed (event type)
	public required TTarget Source { get; init; }

	public int Depth { get; private set; }
	public Func<Event, bool>? Handler;
	List<EventTree<TTarget>>? children;
	public EventTree<TTarget>? Parent { get; private set; }
	public IReadOnlyList<EventTree<TTarget>> Children => children ?? (IReadOnlyList<EventTree<TTarget>>)Array.Empty<EventTree<TTarget>>();
	
	public void Add ( EventTree<TTarget> child ) {
		(children ??= new()).Add( child );
		child.Depth = children.Count - 1;
		child.Parent = this;
	}
	public void Remove ( EventTree<TTarget> child ) {
		var index = children!.IndexOf( child );
		children!.RemoveAt( index );
		child.Parent = null;
		for ( int i = index; i < children.Count; i++ ) {
			children[i].Depth--;
		}
	}

	public void Sort ( Comparison<EventTree<TTarget>> comparer ) {
		children!.Sort( comparer );
		for ( int i = 0; i < children.Count; i++ ) {
			children[i].Depth = i;
		}
	}

	/// <summary>
	/// Returns the next child as if performing a depth-first enumeration where the parent is yielded before its children.
	/// </summary>
	public EventTree<TTarget>? Next {
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

	/// <summary>
	/// Returns the next child with a handler as if performing a depth-first enumeration where the parent is yielded before its children.
	/// </summary>
	public EventTree<TTarget>? NextWithHandler {
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

	/// <summary>
	/// Returns the previous child as if performing a depth-first enumeration where the parent is yielded before its children.
	/// </summary>
	public EventTree<TTarget>? Previous {
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

	/// <summary>
	/// Returns the previous child with a handler as if performing a depth-first enumeration where the parent is yielded before its children.
	/// </summary>
	public EventTree<TTarget>? PreviousWithHandler {
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

	public IEnumerable<EventTree<TTarget>> EnumerateHandlers () {
		var node = this;

		if ( node.Handler != null )
			yield return node;

		loop:
		if ( node.Children.Count != 0 ) {
			node = node.Children[0];
			if ( node.Handler != null )
				yield return node;
			goto loop;
		}

		var parent = node.Parent;
		while ( parent != null ) {
			if ( parent.Children.Count > node.Depth + 1 ) {
				node = parent.Children[node.Depth + 1];
				if ( node.Handler != null )
					yield return node;
				goto loop;
			}

			node = parent;
			parent = node.Parent;
		}
	}

	public IEnumerable<EventTree<TTarget>> EnumerateCulledHandlers ( Func<TTarget, bool> predicate ) {
		var node = this;

		if ( !predicate( node.Source ) )
			yield break;
		if ( node.Handler != null )
			yield return node;

		loop:
		for ( int i = 0; i < node.Children.Count; i++ ) {
			var child = node.Children[i];
			if ( predicate( child.Source ) ) {
				node = node.Children[i];
				if ( node.Handler != null )
					yield return node;
				goto loop;
			}
		}

		var parent = node.Parent;
		while ( parent != null ) {
			for ( int i = node.Depth + 1; i < parent.Children.Count; i++ ) {
				var child = parent.Children[i];
				if ( predicate( child.Source ) ) {
					node = parent.Children[i];
					if ( node.Handler != null )
						yield return node;
					goto loop;
				}
			}

			node = parent;
			parent = node.Parent;
		}
	}

	public IEnumerable<EventTree<TTarget>> EnumerateCulledHandlers<TData> ( TData data, Func<TTarget, TData, bool> predicate ) {
		var node = this;

		if ( !predicate( node.Source, data ) )
			yield break;
		if ( node.Handler != null )
			yield return node;

		loop:
		for ( int i = 0; i < node.Children.Count; i++ ) {
			var child = node.Children[i];
			if ( predicate( child.Source, data ) ) {
				node = node.Children[i];
				if ( node.Handler != null )
					yield return node;
				goto loop;
			}
		}

		var parent = node.Parent;
		while ( parent != null ) {
			for ( int i = node.Depth + 1; i < parent.Children.Count; i++ ) {
				var child = parent.Children[i];
				if ( predicate( child.Source, data ) ) {
					node = parent.Children[i];
					if ( node.Handler != null )
						yield return node;
					goto loop;
				}
			}

			node = parent;
			parent = node.Parent;
		}
	}

	public TTarget? TriggerEvent ( Event @event ) {
		foreach ( var node in EnumerateHandlers() ) {
			if ( node.Handler!( @event ) )
				return node.Source;
		}

		return null;
	}

	public TTarget? TriggerCulledEvent ( Event @event, Func<TTarget, bool> predicate ) {
		foreach ( var node in EnumerateCulledHandlers( predicate ) ) {
			if ( node.Handler!( @event ) )
				return node.Source;
		}

		return null;
	}

	public TTarget? TriggerCulledEvent<TData> ( Event @event, TData data, Func<TTarget, TData, bool> predicate ) {
		foreach ( var node in EnumerateCulledHandlers( data, predicate ) ) {
			if ( node.Handler!( @event ) )
				return node.Source;
		}

		return null;
	}

	public void CreateHandlerQueue ( ICollection<EventTree<TTarget>> collection ) {
		foreach ( var node in EnumerateHandlers() )
			collection.Add( node );
	}

	public void CreateCulledHandlerQueue ( ICollection<EventTree<TTarget>> collection, Func<TTarget, bool> predicate ) {
		foreach ( var node in EnumerateCulledHandlers( predicate ) )
			collection.Add( node );
	}

	public void CreateCulledHandlerQueue<TData> ( ICollection<EventTree<TTarget>> collection, TData data, Func<TTarget, TData, bool> predicate ) {
		foreach ( var node in EnumerateCulledHandlers( data, predicate ) )
			collection.Add( node );
	}
}