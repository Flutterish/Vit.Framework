using Vit.Framework.DependencyInjection;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public abstract class CompositeUIComponent : CompositeUIComponent<UIComponent> { }
public abstract class CompositeUIComponent<T> : UIComponent, ICompositeUIComponent<T>, IHasCompositeDrawNodes<DrawNode> where T : UIComponent {
	List<T> internalChildren = new();
	public IReadOnlyList<T> Children {
		get => internalChildren;
		protected set {
			ClearInternalChildren( dispose: true );
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}
	public IReadOnlyDependencyCache Dependencies { get; private set; } = null!;

	protected void AddInternalChild ( T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = internalChildren.Count;
		internalChildren.Add( child );
		if ( IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}
	protected void InsertInternalChild ( int index, T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = index;
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Depth++;
		}
		internalChildren.Insert( index, child );
		if ( IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}

	protected void RemoveInternalChild ( T child ) {
		var index = child.Depth;
		if ( internalChildren[index] != child )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		RemoveInternalChildAt( index );
	}
	protected void RemoveInternalChildAt ( int index ) {
		var child = internalChildren[index];

		child.Parent = null;
		internalChildren.RemoveAt( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Depth--;
		}
		if ( child.IsLoaded )
			child.Unload();
		onChildRemoved( child );
	}

	protected void ClearInternalChildren ( bool dispose ) {
		while ( internalChildren.Any() ) {
			var child = internalChildren[^1];
			RemoveInternalChildAt( internalChildren.Count - 1 );
			if ( dispose )
				child.Dispose();
		}
	}

	void onChildAdded ( T child ) {
		child.EventHandlerAdded += onChildEventHandlerAdded;
		child.EventHandlerRemoved += onChildEventHandlerRemoved;
		foreach ( var (type, tree) in child.HandledEventTypes ) {
			onChildEventHandlerAdded( type, tree );
		}
		ChildAdded?.Invoke( this, child );
		InvalidateLayout( LayoutInvalidations.Children );
		invalidateDrawNodes();
	}

	void onChildRemoved ( T child ) {
		child.EventHandlerAdded -= onChildEventHandlerAdded;
		child.EventHandlerRemoved -= onChildEventHandlerRemoved;
		foreach ( var (type, tree) in child.HandledEventTypes ) {
			onChildEventHandlerRemoved( type, tree );
		}
		ChildRemoved?.Invoke( this, child );
		InvalidateLayout( LayoutInvalidations.Children );
		invalidateDrawNodes();
	}

	void onChildEventHandlerRemoved ( Type type, EventTree<UIComponent> tree ) {
		var ourTree = GetEventTree( type );
		ourTree.Children!.Remove( tree );
		TryTrimEventTree( type );

		sortEventTree( ourTree );
	}

	void onChildEventHandlerAdded ( Type type, EventTree<UIComponent> tree ) {
		var ourTree = GetEventTree( type );
		ourTree.Children ??= new();
		ourTree.Children.Add( tree );

		sortEventTree( ourTree );
	}

	void sortEventTree ( EventTree<UIComponent> tree ) {
		tree.Children!.Sort( static ( a, b ) => a.Source.Depth - b.Source.Depth );
	}

	protected virtual IReadOnlyDependencyCache CreateDependencies ( IReadOnlyDependencyCache parent ) {
		return parent;
	}

	protected RenderThreadScheduler RenderThreadScheduler { get; private set; } = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		RenderThreadScheduler = dependencies.Resolve<RenderThreadScheduler>();
		Dependencies = CreateDependencies( dependencies );
		foreach ( var i in internalChildren ) {
			i.Load( Dependencies );
		}
	}
	protected override void OnUnload () {
		foreach ( var i in internalChildren.Reverse<T>() ) {
			i.Unload();
		}
		Dependencies = null!;
	}

	public override void Update () {
		base.Update();
		updateSubtree();
	}

	void updateSubtree () {
		foreach ( var i in internalChildren ) {
			i.Update();
		}
	}

	protected override void OnDispose () {
		RenderThreadScheduler.ScheduleDrawNodeDisposal( this );
		foreach ( var i in internalChildren.Reverse<T>() ) {
			i.Dispose();
		}
		base.OnDispose();
	}

	protected override void OnMatrixInvalidated () {
		foreach ( var i in internalChildren ) {
			i.OnParentMatrixInvalidated();
		}
	}

	public virtual void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations ) {
		InvalidateLayout( LayoutInvalidations.Children );
	}

	protected sealed override void PerformLayout () {
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Self ) ) {
			PerformSelfLayout();
		}
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Children ) ) {
			foreach ( var i in Children ) {
				i.ComputeLayout();
			}
		}
	}

	protected virtual void PerformSelfLayout () { }

	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildRemoved;

	void invalidateDrawNodes () {
		if ( drawNodeInvalidations.InvalidateDrawNodes() )
			Parent?.OnChildDrawNodesInvalidated();
	}
	public void OnChildDrawNodesInvalidated () {
		invalidateDrawNodes();
	}
	DrawNodeInvalidations drawNodeInvalidations;
	DrawNode?[] drawNodes = new DrawNode?[3];
	public override Rendering.DrawNode GetDrawNode ( int subtreeIndex ) {
		if ( internalChildren.Count == 1 ) {
			drawNodeInvalidations.ValidateDrawNode( subtreeIndex );
			return internalChildren[0].GetDrawNode( subtreeIndex );
		}

		var node = drawNodes[subtreeIndex] ??= new DrawNode( this, subtreeIndex );
		node.Update();
		return node;
	}

	public override void DisposeDrawNodes () {
		foreach ( var i in drawNodes ) {
			i?.Dispose();
		}
	}

	public class DrawNode : CompositeDrawNode<CompositeUIComponent<T>, Rendering.DrawNode> {
		public DrawNode ( CompositeUIComponent<T> source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override bool ValidateChildList () {
			return Source.drawNodeInvalidations.ValidateDrawNode( SubtreeIndex );
		}
	}

	public IReadOnlyList<IHasDrawNodes<Rendering.DrawNode>> CompositeDrawNodeSources => internalChildren;
}

public interface ICompositeUIComponent<out T> : IUIComponent, IReadOnlyCompositeComponent<UIComponent, T> where T : UIComponent {
	IReadOnlyDependencyCache Dependencies { get; }

	void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations );
	void OnChildDrawNodesInvalidated ();

	new public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildAdded;
	new public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildRemoved;

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<UIComponent, T>, T>? IReadOnlyCompositeComponent<UIComponent, T>.ChildAdded {
		add => ChildAdded += value;
		remove => ChildAdded -= value;
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<UIComponent, T>, T>? IReadOnlyCompositeComponent<UIComponent, T>.ChildRemoved {
		add => ChildRemoved += value;
		remove => ChildRemoved -= value;
	}
}

[Flags]
public enum LayoutInvalidations { // TODO we need flags for when the parent needs to recompute its own layout or just propagate the update
	None = 0,
	/// <summary>
	/// Some layout parameter of this component changed. This will force this component to recalculate its layout.
	/// </summary>
	Self = 1,
	/// <summary>
	/// Some layout parameter of this components children changed. This will force this component to update the layout of its children.
	/// </summary>
	Children = 2,
	/// <summary>
	/// The required size for this component has changed.
	/// </summary>
	RequiredSize = 4
}