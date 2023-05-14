using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

/// <summary>
/// A container which controls the layout of child <see cref="ILayoutElement"/>s.
/// </summary>
public interface ILayoutContainer {
	/// <summary>
	/// The size available to lay out child elements in, in local space.
	/// </summary>
	/// <remarks>
	/// If this container is also a <see cref="ILayoutElement"/>, it might be equal to <see cref="ILayoutElement.Size"/>.
	/// </remarks>
	Size2<float> ContentSize { get; }
}

/// <inheritdoc cref="ILayoutContainer"/>
public interface ILayoutContainer<T> : ILayoutContainer<T, T>, IContainer<T> where T : Drawable, ILayoutElement {

}

/// <inheritdoc cref="ILayoutContainer"/>
public interface ILayoutContainer<in T, out TChild> : ILayoutContainer, IContainer<T, TChild> where TChild : Drawable, ILayoutElement where T : TChild {

}