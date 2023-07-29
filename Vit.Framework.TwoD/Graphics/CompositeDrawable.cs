using Vit.Framework.DependencyInjection;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

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
			throw new InvalidOperationException( "A drawable may only have 1 parent" );

		child.SetParent( (ICompositeDrawable<IDrawable>)this );
		child.SetDepth( internalChildren.Count );
		internalChildren.Add( child );
		if ( IsLoaded )
			child.TryLoad( dependencies );
		ChildAdded?.Invoke( this, child );
		InvalidateDrawNodes();
	}

	protected void InsertInternalChild ( T child, int index ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "A drawable may only have 1 parent" );

		child.SetParent( (ICompositeDrawable<IDrawable>)this );
		child.SetDepth( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].SetDepth( i + 1 );
		}
		internalChildren.Insert( index, child );
		if ( IsLoaded )
			child.TryLoad( dependencies );
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
		ChildRemoved?.Invoke( this, child );
		InvalidateDrawNodes();
	}

	protected void ClearInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			var child = internalChildren[^1];
			child.SetParent( null );
			internalChildren.RemoveAt( internalChildren.Count - 1 );
			ChildRemoved?.Invoke( this, child );
		}

		InvalidateDrawNodes();
	}

	protected void DisposeInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			var child = internalChildren[^1];
			child.SetParent( null );
			internalChildren.RemoveAt( internalChildren.Count - 1 );
			ChildRemoved?.Invoke( this, child );
			child.Dispose();
		}

		InvalidateDrawNodes();
	}

	public IReadOnlyDependencyCache Dependencies => dependencies;
	IDependencyCache dependencies = null!;
	protected override void Load ( IReadOnlyDependencyCache dependencies ) {
		base.Load( dependencies );

		this.dependencies = CreateDependencies( dependencies );
		foreach ( var i in internalChildren ) {
			i.TryLoad( this.dependencies );
		}
	}

	protected virtual IDependencyCache CreateDependencies ( IReadOnlyDependencyCache parentDependencies ) => new DependencyCache( parentDependencies );

	protected override void OnMatrixInvalidated () {
		base.OnMatrixInvalidated();

		foreach ( var i in internalChildren )
			i.OnParentMatrixInvalidated();
	}

	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
		foreach ( var i in Children ) {
			i.Dispose();
		}
	}

	public IReadOnlyList<IHasDrawNodes<Rendering.DrawNode>> CompositeDrawNodeSources => (IReadOnlyList<IHasDrawNodes<Rendering.DrawNode>>)internalChildren;
	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	public class DrawNode : CompositeDrawNode<CompositeDrawable<T>, Rendering.DrawNode> {
		public DrawNode ( CompositeDrawable<T> source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override bool ValidateChildList () {
			return Source.DrawNodeInvalidations.ValidateDrawNode( SubtreeIndex );
		}
	}
}

public interface ICompositeDrawable<out T> : IDrawable, IReadOnlyCompositeComponent<IDrawable, T>, IHasCompositeDrawNodes<DrawNode> where T : IDrawable {
	IReadOnlyDependencyCache Dependencies { get; }

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