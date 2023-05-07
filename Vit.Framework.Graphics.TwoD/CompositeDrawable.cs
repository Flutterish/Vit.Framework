using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Hierarchy;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public abstract class CompositeDrawable<T> : Drawable, ICompositeDrawable<T> where T : Drawable, IDrawable {
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

		child.SetParent( this );
		internalChildren.Add( child );
		ChildAdded?.Invoke( this, child );
		InvalidateDrawNodes();
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

		child.SetParent( null );
		internalChildren.Remove( child );
		ChildRemoved?.Invoke( this, child );
		InvalidateDrawNodes();
		return true;
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

	protected override void OnMatrixInvalidated () {
		foreach ( var i in internalChildren )
			i.OnParentMatrixInvalidated();
	}

	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;

	protected override Drawable.DrawNode CreateDrawNode ( int subtreeIndex ) {
		throw new NotImplementedException();
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