using System.Collections;
using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class Container<T> : CompositeDrawable<T>, IContainer<T> where T : Drawable {
	public T Child {
		get => Children.Single();
		set {
			ClearChildren();
			AddChild( value );
		}
	}

	new public IEnumerable<T> Children {
		get => base.Children;
		set {
			ClearChildren();
			this.AddChildren( value );
		}
	}

	public void AddChild ( T child ) {
		AddInternalChild( child );
	}

	public bool RemoveChild ( T child ) {
		return RemoveInternalChild( child );
	}

	public void ClearChildren () {
		ClearInternalChildren();
	}
}

public abstract class Container<T, TInternal> : CompositeDrawable<TInternal>, IContainer<T> where T : Drawable where TInternal : Drawable {
	protected abstract IContainer<T> Source { get; }

	public void AddChild ( T child ) {
		Source.AddChild( child );
	}

	public bool RemoveChild ( T child ) {
		return Source.RemoveChild( child );
	}

	public void ClearChildren () {
		Source.ClearChildren();
	}

	IEnumerable<T> IReadOnlyCompositeComponent<T>.Children => Source.Children;
	IEnumerator IEnumerable.GetEnumerator () {
		return Source.GetEnumerator();
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? IReadOnlyCompositeComponent<T>.ChildAdded {
		add {
			Source.ChildAdded += value;
		}

		remove {
			Source.ChildAdded -= value;
		}
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? IReadOnlyCompositeComponent<T>.ChildRemoved {
		add {
			Source.ChildRemoved += value;
		}

		remove {
			Source.ChildRemoved -= value;
		}
	}
}

public interface IContainer<T> : IContainer<T, T> where T : Drawable { }

public interface IContainer<in T, out TChild> : IDrawable, ICompositeComponent<Drawable, T, TChild>
	where T : Drawable, TChild
	where TChild : Drawable {

}