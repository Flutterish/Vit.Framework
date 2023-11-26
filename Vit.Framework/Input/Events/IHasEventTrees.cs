using System.Reflection;
using System.Runtime.CompilerServices;
using Vit.Framework.Hierarchy;

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
	public static bool TriggerEventOnSelf<TSelf> ( this TSelf self, Event @event ) where TSelf : class, IHasEventTrees<TSelf> {
		var type = @event.GetType();
		return self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.Handler is Func<Event, bool> handler && handler( @event );
	}

	public static TSelf? TriggerEventUpTree<TSelf> ( this TSelf self, Event @event ) where TSelf : class, IHasEventTrees<TSelf>, IComponent<TSelf> {
		var type = @event.GetType();
		do {
			if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.Handler is Func<Event, bool> handler && handler( @event ) )
				return self;
			self = (TSelf)self.Parent!;
		}
		while ( self != null );

		return null;
	}

	public static TSelf? TriggerEvent<TSelf, TCuller> ( this TSelf self, Event @event, TCuller culler ) where TSelf : class, IHasEventTrees<TSelf> where TCuller : struct, EventTree<TSelf>.IEventTreeCuller {
		var type = @event.GetType();
		if ( self.HandledEventTypes.TryGetValue( type, out var tree ) && tree.TriggerEvent( @event, culler ) is TSelf handler )
			return handler;

		return null;
	}

	public static TSelf? TriggerEvent<TSelf> ( this TSelf self, Event @event ) where TSelf : class, IHasEventTrees<TSelf> {
		return self.TriggerEvent( @event, new EventTree<TSelf>.NullEventTreeCuller() );
	}

	public static TSelf? TriggerCulledEvent<TSelf> ( this TSelf self, Event @event, Func<TSelf, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		return self.TriggerEvent( @event, new EventTree<TSelf>.PredicateEventTreeCuller { Predicate = predicate } );
	}

	public static TSelf? TriggerCulledEvent<TSelf, TData> ( this TSelf self, Event @event, TData data, Func<TSelf, TData, bool> predicate ) where TSelf : class, IHasEventTrees<TSelf> {
		return self.TriggerEvent( @event, new EventTree<TSelf>.PredicateWithDataEventTreeCuller<TData> { Data = data, Predicate = predicate } );
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
	/// Returns the next node as if performing a depth-first enumeration where the parent is yielded before its children.
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
	/// Returns the next node with a handler as if performing a depth-first enumeration where the parent is yielded before its children.
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
	/// Returns the previous node as if performing a depth-first enumeration where the parent is yielded before its children.
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
	/// Returns the previous node with a handler as if performing a depth-first enumeration where the parent is yielded before its children.
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

	/// <summary>
	/// Returns the next node as if performing a depth-first enumeration where the children are yielded before the parent.
	/// </summary>
	public EventTree<TTarget>? ChildFirstNext {
		get {
			var parent = Parent;

			if ( parent != null && parent.Children.Count > Depth + 1 ) {
				var node = parent.Children[Depth + 1];
				while ( node.Children.Count != 0 ) {
					node = node.Children[0];
				}

				return node;
			}
			return parent;
		}
	}

	/// <summary>
	/// Returns the next node with a handler as if performing a depth-first enumeration where the children are yielded before the parent.
	/// </summary>
	public EventTree<TTarget>? ChildFirstNextWithHandler {
		get {
			var node = Next;
			while ( node != null ) {
				if ( node.Handler != null )
					return node;

				node = node.ChildFirstNext;
			}

			return null;
		}
	}

	/// <summary>
	/// Returns the previous node as if performing a depth-first enumeration where the children are yielded before the parent.
	/// </summary>
	public EventTree<TTarget>? ChildFirstPrevious {
		get {
			if ( Children.Count != 0 ) {
				return Children[^1];
			}

			var parent = Parent;
			var node = this;
			while ( parent != null ) {
				if ( node.Depth != 0 ) {
					return parent.Children[node.Depth - 1];
				}

				node = parent;
				parent = node.Parent;
			}

			return null;
		}
	}

	/// <summary>
	/// Returns the previous node with a handler as if performing a depth-first enumeration where the children are yielded before the parent.
	/// </summary>
	public EventTree<TTarget>? ChildFirstPreviousWithHandler {
		get {
			var node = Previous;
			while ( node != null ) {
				if ( node.Handler != null )
					return node;

				node = node.ChildFirstPrevious;
			}

			return null;
		}
	}

	public bool ShouldBeCulled => Handler == null && Children.Count == 0;

	public interface IEventTreeCuller {
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		bool ShouldNotBeCulled ( EventTree<TTarget> node );
	}

	public struct NullEventTreeCuller : IEventTreeCuller {
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public readonly bool ShouldNotBeCulled ( EventTree<TTarget> node ) {
			return true;
		}
	}

	public struct PredicateEventTreeCuller : IEventTreeCuller {
		public required Func<TTarget, bool> Predicate;

		public bool ShouldNotBeCulled ( EventTree<TTarget> node ) {
			return Predicate( node.Source );
		}
	}

	public struct PredicateWithDataEventTreeCuller<TData> : IEventTreeCuller {
		public required Func<TTarget, TData, bool> Predicate;
		public required TData Data;

		public bool ShouldNotBeCulled ( EventTree<TTarget> node ) {
			return Predicate( node.Source, Data );
		}
	}

	/// <summary>
	/// Performs a depth-first enumeration where the parent is yielded before its children.
	/// </summary>
	public IEnumerable<EventTree<TTarget>> EnumerateHandlers<TCuller> ( TCuller culler ) where TCuller : struct, IEventTreeCuller {
		var node = this;

		if ( !culler.ShouldNotBeCulled( node ) )
			yield break;
		if ( node.Handler != null )
			yield return node;

		loop:
		for ( int i = 0; i < node.Children.Count; i++ ) {
			var child = node.Children[i];
			if ( culler.ShouldNotBeCulled( child ) ) {
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
				if ( culler.ShouldNotBeCulled( child ) ) {
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

	/// <summary>
	/// Performs a depth-first enumeration where the children are yielded before the parent.
	/// </summary>
	public IEnumerable<EventTree<TTarget>> EnumerateHandlersChildFirst<TCuller> ( TCuller culler ) where TCuller : struct, IEventTreeCuller {
		var node = this;

		if ( !culler.ShouldNotBeCulled( node ) )
			yield break;

		while ( node.Children.Count != 0 && culler.ShouldNotBeCulled( node.Children[0] ) ) {
			node = node.Children[0];
		}

		if ( node.Handler != null )
			yield return node;

		loop:
		var parent = node.Parent;
		if ( parent != null ) {
			for ( int i = node.Depth + 1; i < parent.Children.Count; i++ ) {
				var child = parent.Children[i];
				if ( !culler.ShouldNotBeCulled( child ) )
					continue;

				while ( child.Children.Count != 0 && culler.ShouldNotBeCulled( child.Children[0] ) ) {
					child = child.Children[0];
				}

				node = child;
				if ( node.Handler != null )
					yield return node;
				goto loop;
			}

			node = parent;
			if ( node.Handler != null )
				yield return node;
			goto loop;
		}
	}

	public TTarget? TriggerEvent<TCuller> ( Event @event, TCuller culler ) where TCuller : struct, IEventTreeCuller {
		foreach ( var node in EnumerateHandlers( culler ) ) {
			if ( node.Handler!( @event ) )
				return node.Source;
		}

		return null;
	}

	public TTarget? TriggerEventChildFirst<TCuller> ( Event @event, TCuller culler ) where TCuller : struct, IEventTreeCuller {
		foreach ( var node in EnumerateHandlersChildFirst( culler ) ) {
			if ( node.Handler!( @event ) )
				return node.Source;
		}

		return null;
	}

	public void CreateHandlerQueue<TCuller> ( ICollection<EventTree<TTarget>> collection, TCuller culler ) where TCuller : struct, IEventTreeCuller {
		foreach ( var node in EnumerateHandlers( culler ) )
			collection.Add( node );
	}

	public void CreateChildFirstHandlerQueue<TCuller> ( ICollection<EventTree<TTarget>> collection, TCuller culler ) where TCuller : struct, IEventTreeCuller {
		foreach ( var node in EnumerateHandlersChildFirst( culler ) )
			collection.Add( node );
	}
}
