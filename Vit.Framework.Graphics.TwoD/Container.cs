using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD;

public class Container<T> : CompositeDrawable<T>, IContainer<T, T> where T : Drawable {
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

public interface IContainer<in T, out TChild> : ICompositeComponent<Drawable, T, TChild> 
	where T : Drawable, TChild
	where TChild : Drawable
{

}