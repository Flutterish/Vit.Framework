using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public abstract class CompositeDrawable<T> : Drawable, ICompositeDrawable<T> where T : IDrawable {
	readonly List<T> internalChildren = new();
	public IReadOnlyList<T> Children => internalChildren;

	protected void AddInternalChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			AddInternalChild( i );
	}
	protected void AddInternalChildren ( params T[] children ) {
		foreach ( var i in children )
			AddInternalChild( i );
	}
	protected void AddInternalChild ( T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "A drawable might only have 1 parent" );

		child.SetParent( (ICompositeDrawable<IDrawable>)this );
		internalChildren.Add( child );
		addChildEventHandlers( child );
		if ( IsLoaded )
			child.TryLoad();
		ChildAdded?.Invoke( this, child );
		InvalidateDrawNodes();
	}

	Dictionary<Type, Func<Event, bool>>? eventHandlers;
	/// <summary>
	/// Adds an event handler for events of type T. The handler should return <see langword="true"/> to stop propagation, <see langword="false"/> otherwise.
	/// </summary>
	override protected void AddEventHandler ( Type type, Func<Event, bool> handler ) {
		eventHandlers ??= new();
		eventHandlers.Add( type, handler );

		addEventTypeHandler( type );
	}

	override protected void RemoveEventHandler ( Type type ) {
		eventHandlers!.Remove( type );

		if ( eventHandlingChildren.ContainsKey( type ) ) {
			addEventTypeHandler( type );
		}
		else {
			removeEventTypeHandler( type );
		}
	}

	void addEventTypeHandler ( Type type ) {
		removeEventTypeHandler( type );

		var selfHandler = eventHandlers?.GetValueOrDefault( type );
		var childHandlers = eventHandlingChildren.GetValueOrDefault( type );

		if ( selfHandler == null ) {
			base.AddEventHandler( type, e => {
				foreach ( var i in childHandlers! ) {
					if ( i.Value.Invoke( e ) )
						return true;
				}

				return false;
			} );
		}
		else if ( childHandlers == null ) {
			base.AddEventHandler( type, selfHandler );
		}
		else {
			base.AddEventHandler( type, e => {
				if ( selfHandler( e ) )
					return true;

				foreach ( var i in childHandlers ) {
					if ( i.Value.Invoke( e ) )
						return true;
				}

				return false;
			} );
		}
	}

	void removeEventTypeHandler ( Type type ) {
		if ( !HandlesEventType( type ) )
			return;

		base.RemoveEventHandler( type );
	}

	Dictionary<Type, SortedList<IEventHandlingComponent, Func<Event, bool>>> eventHandlingChildren = new();
	void onChildEventHandlerRemoved ( IEventHandlingComponent child, Type type ) {
		var set = eventHandlingChildren[type];
		set.Remove( child );
		if ( set.Count == 0 ) {
			eventHandlingChildren.Remove( type );
			if ( eventHandlers?.ContainsKey( type ) != true ) {
				removeEventTypeHandler( type );
			}
			else {
				addEventTypeHandler( type );
			}
		}
	}

	void onChildEventHandlerAdded ( IEventHandlingComponent child, Type type, Func<Event, bool> handler ) {
		if ( !eventHandlingChildren.TryGetValue( type, out var set ) ) { // TODO can improve sorting by storing order in children
			eventHandlingChildren.Add( type, set = new( Comparer<IEventHandlingComponent>.Create( (a,b) => internalChildren.IndexOf((T)b) - internalChildren.IndexOf((T)a) ) ) ); // TODO probably pool these
			addEventTypeHandler( type );
		}

		set.Add( child, handler );
	}

	void addChildEventHandlers ( T child ) {
		child.EventHandlerAdded += onChildEventHandlerAdded;
		child.EventHandlerRemoved += onChildEventHandlerRemoved;
		foreach ( var i in child.HandledEventTypes ) {
			onChildEventHandlerAdded( child, i.Key, i.Value );
		}
	}

	void removeChildEventHandlers ( T child ) {
		child.EventHandlerAdded -= onChildEventHandlerAdded;
		child.EventHandlerRemoved -= onChildEventHandlerRemoved;
		foreach ( var i in child.HandledEventTypes ) {
			onChildEventHandlerRemoved( child, i.Key );
		}
	}

	protected void InsertInternalChild ( T child, int index ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "A drawable might only have 1 parent" );

		child.SetParent( (ICompositeDrawable<IDrawable>)this );
		internalChildren.Insert( index, child );
		addChildEventHandlers( child );
		if ( IsLoaded )
			child.TryLoad();
		ChildAdded?.Invoke( this, child );
		InvalidateDrawNodes();
	}

	protected void RemoveInternalChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			RemoveInternalChild( i );
	}
	protected void RemoveInternalChildren ( params T[] children ) {
		foreach ( var i in children )
			RemoveInternalChild( i );
	}
	protected bool RemoveInternalChild ( T child ) {
		if ( child.Parent == null )
			return false;

		if ( child.Parent != this )
			throw new InvalidOperationException( "This child does not belong to this parent" );

		child.SetParent( null );
		internalChildren.Remove( child );
		removeChildEventHandlers( child );
		ChildRemoved?.Invoke( this, child );
		InvalidateDrawNodes();
		return true;
	}
	protected void RemoveInternalChildAt ( int index ) {
		var child = internalChildren[index];
		if ( child.Parent == null )
			return;

		if ( child.Parent != this )
			throw new InvalidOperationException( "This child does not belong to this parent" );

		child.SetParent( null );
		internalChildren.RemoveAt( index );
		removeChildEventHandlers( child );
		ChildRemoved?.Invoke( this, child );
		InvalidateDrawNodes();
	}

	protected void ClearInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			var child = internalChildren[^1];
			child.SetParent( null );
			internalChildren.RemoveAt( internalChildren.Count - 1 );
			removeChildEventHandlers( child );
			ChildRemoved?.Invoke( this, child );
		}

		InvalidateDrawNodes();
	}

	public override void Update () {
		UpdateSubtree();
	}

	protected void UpdateSubtree () {
		foreach ( var i in internalChildren ) {
			i.Update();
		}
	}

	public IReadonlyDependencyCache Dependencies => dependencies;
	IDependencyCache dependencies = null!;
	protected override void Load () {
		dependencies = CreateDependencies();
		foreach ( var i in internalChildren ) {
			i.TryLoad();
		}
	}

	protected virtual IDependencyCache CreateDependencies () => new DependencyCache( Parent?.Dependencies );

	protected override void OnMatrixInvalidated () {
		base.OnMatrixInvalidated();

		foreach ( var i in internalChildren )
			i.OnParentMatrixInvalidated();
	}

	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;

	protected override Drawable.DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	new public class DrawNode : Drawable.DrawNode {
		new protected CompositeDrawable<T> Source => (CompositeDrawable<T>)base.Source;
		public DrawNode ( CompositeDrawable<T> source, int subtreeIndex ) : base( source, subtreeIndex ) {
			ChildNodes = new( source.internalChildren.Count );
		}

		protected RentedArray<Drawable.DrawNode> ChildNodes;
		protected override void UpdateState () {
			var count = Source.internalChildren.Count;
			ChildNodes.ReallocateStorage( count );
			for ( int i = 0; i < count; i++ ) {
				ChildNodes[i] = Source.internalChildren[i].GetDrawNode( SubtreeIndex );
			}
		}

		public override void Draw ( ICommandBuffer commands ) {
			foreach ( var i in ChildNodes.AsSpan() ) {
				i.Draw( commands );
			}
		}

		public override void ReleaseResources ( bool willBeReused ) {
			if ( willBeReused )
				return;

			ChildNodes.Dispose();
		}
	}
}

public interface ICompositeDrawable<out T> : IDrawable, IReadOnlyCompositeComponent<IDrawable, T> where T : IDrawable {
	IReadonlyDependencyCache Dependencies { get; }

	new public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	new public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<IDrawable, T>, T>? IReadOnlyCompositeComponent<IDrawable, T>.ChildAdded {
		add => ChildAdded += value;
		remove => ChildAdded -= value;
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<IDrawable, T>, T>? IReadOnlyCompositeComponent<IDrawable, T>.ChildRemoved {
		add => ChildRemoved += value;
		remove => ChildRemoved -= value;
	}
}