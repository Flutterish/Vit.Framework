using Vit.Framework.Hierarchy;

namespace Vit.Framework.Graphics.TwoD;

public abstract class Drawable : IDrawable {
	public ICompositeDrawable<IDrawable>? Parent { get; }
}

public interface IDrawable : IComponent<IDrawable> {
	new ICompositeDrawable<IDrawable>? Parent { get; }
	IReadOnlyCompositeComponent<IDrawable, IDrawable>? IComponent<IDrawable>.Parent => Parent;
}