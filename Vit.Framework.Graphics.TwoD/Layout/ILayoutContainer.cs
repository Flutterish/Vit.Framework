using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

/// <summary>
/// A container which controls the layout of child <see cref="ILayoutElement"/>s.
/// </summary>
public interface ILayoutContainer : IDrawable {
	/// <summary>
	/// The size available to lay out child elements in, in local space.
	/// This accounts for <see cref="Padding"/>.
	/// </summary>
	Size2<float> ContentSize { get; }

	/// <summary>
	/// Padding provides spacing between the container edges and elements. 
	/// It is in-set into the container, so that the container layout size does not change because of padding.
	/// </summary>
	Spacing<float> Padding { get; set; }
}

/// <inheritdoc cref="ILayoutContainer"/>
public interface ILayoutContainer<out T> : ILayoutContainer, ICompositeDrawable<T> where T : ILayoutElement {

}

public interface ILayoutContainer<in T, in TParam, out TChild> : ILayoutContainer<TChild> where T : TChild where TChild : ILayoutElement {
	void AddChild ( T child, TParam param );
	bool RemoveChild ( T child );
	void ClearChildren ();
}