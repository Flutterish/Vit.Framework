using System.Collections;
using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class Container<T> : CompositeDrawable<T>, IContainer<T> where T : IDrawable {
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

	public IReadOnlyList<T> ChildList {
		get => base.Children;
		set {
			ClearChildren();
			this.AddChildren( value );
		}
	}

	public void AddChild ( T child ) {
		AddInternalChild( child );
	}

	public void InsertChild ( T child, int index ) {
		InsertInternalChild( child, index );
	}

	public bool RemoveChild ( T child ) {
		return RemoveInternalChild( child );
	}

	public void RemoveChildAt ( int index ) {
		RemoveInternalChildAt( index );
	}

	public void ClearChildren () {
		ClearInternalChildren();
	}

	public void DisposeChildren () {
		DisposeInternalChildren();
	}
}

// Container<T, TInternal>

public interface IContainer<T> : IContainer<T, T> where T : IDrawable { }

public interface IContainer<in T, out TChild> : IDrawable, ICompositeComponent<IDrawable, T, TChild>
	where T : TChild
	where TChild : IDrawable {

}