using System.Runtime.InteropServices;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Hierarchy;
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
		if ( IsLoaded )
			child.TryLoad();
		ChildAdded?.Invoke( this, child );
		InvalidateDrawNodes();
	}
	protected void InsertInternalChild ( T child, int index ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "A drawable might only have 1 parent" );

		child.SetParent( (ICompositeDrawable<IDrawable>)this );
		internalChildren.Insert( index, child );
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