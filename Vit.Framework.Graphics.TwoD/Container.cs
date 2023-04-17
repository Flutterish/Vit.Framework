using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD;

public class Container<T> : CompositeDrawable<T>, IContainer<T, T> where T : IDrawable {
	public void AddChild ( T child ) {
		throw new NotImplementedException();
	}

	public bool RemoveChild ( T child ) {
		throw new NotImplementedException();
	}

	public void ClearChildren () {
		throw new NotImplementedException();
	}
}

public interface IContainer<in T, out TChild> : ICompositeComponent<IDrawable, T, TChild> 
	where T : TChild, IDrawable 
	where TChild : IDrawable
{

}