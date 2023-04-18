using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD;

public abstract class CompositeDrawable<T> : Drawable, ICompositeDrawable<T> where T : Drawable {
	readonly List<T> internalChildren = new();
	public IEnumerable<T> Children => internalChildren;

	public void AddInternalChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			AddInternalChild( i );
	}
	public void AddInternalChildren ( params T[] children ) {
		foreach ( var i in children )
			AddInternalChild( i );
	}
	protected void AddInternalChild ( T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "A drawable might only have 1 parent" );

		child.Parent = this;
		internalChildren.Add( child );
		ChildAdded?.Invoke( this, child );
	}

	public void RemoveInternalChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			RemoveInternalChild( i );
	}
	public void RemoveInternalChildren ( params T[] children ) {
		foreach ( var i in children )
			RemoveInternalChild( i );
	}
	protected bool RemoveInternalChild ( T child ) {
		if ( child.Parent == null )
			return false;

		if ( child.Parent != this )
			throw new InvalidOperationException( "This child does not belong to this parent" );

		child.Parent = null;
		internalChildren.Remove( child );
		ChildRemoved?.Invoke( this, child );
		return true;
	}

	protected void ClearInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			var child = internalChildren[^1];
			child.Parent = null;
			internalChildren.RemoveAt( internalChildren.Count - 1 );
			ChildRemoved?.Invoke( this, child );
		}
	}

	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;
}

public interface ICompositeDrawable<out T> : IDrawable, IReadOnlyCompositeComponent<Drawable, T> where T : Drawable {
	new public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	new public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<Drawable, T>, T>? IReadOnlyCompositeComponent<Drawable, T>.ChildAdded {
		add => ChildAdded += value;
		remove => ChildAdded -= value;
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<Drawable, T>, T>? IReadOnlyCompositeComponent<Drawable, T>.ChildRemoved {
		add => ChildRemoved += value;
		remove => ChildRemoved -= value;
	}
}