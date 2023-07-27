using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

/// <summary>
/// A container which controls the layout of child <see cref="IDrawableLayoutElement"/>s.
/// </summary>
public interface IDrawableLayoutContainer : IDrawable {
	/// <summary>
	/// The size available to lay out child elements in, in local space.
	/// This accounts for <see cref="Padding"/>.
	/// </summary>
	Size2<float> ContentSize { get; }

	/// <summary>
	/// Padding provides spacing between the container edges and elements. 
	/// It is in-set into the container, so that the container layout size does not change because of padding.
	/// </summary>
	/// <remarks>
	/// Padding may be negative in order to display content outside the actual container bounds.
	/// </remarks>
	Spacing<float> Padding { get; set; }
}

/// <inheritdoc cref="IDrawableLayoutContainer"/>
public interface IDrawableLayoutContainer<out T> : IDrawableLayoutContainer, ICompositeDrawable<T> where T : IDrawableLayoutElement {

}

public interface IDrawableLayoutContainer<in T, in TParam, out TChild> : IDrawableLayoutContainer<TChild> where T : TChild where TChild : IDrawableLayoutElement {
	void AddChild ( T child, TParam param );
	bool RemoveChild ( T child );
	void ClearChildren ();
}