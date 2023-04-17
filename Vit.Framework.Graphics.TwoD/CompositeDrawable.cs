using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD;

public abstract class CompositeDrawable<T> : Drawable, ICompositeDrawable<T> where T : IDrawable {
	public IEnumerable<T> Children { get; }

	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeDrawable<T>, T>? ChildRemoved;
}

public interface ICompositeDrawable<out T> : IDrawable, IReadOnlyCompositeComponent<IDrawable, T> where T : IDrawable {
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