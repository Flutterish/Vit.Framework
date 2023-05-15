using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

/// <summary>
/// A layout-able element. The properties of such elements are usually managed (or used) by <see cref="ILayoutContainer"/>s.
/// </summary>
public interface ILayoutElement : IDrawable {
	/// <summary>
	/// The size allocated for this elements layout, in parent space.
	/// </summary>
	Size2<float> Size { get; set; }

	/// <summary>
	/// The minumum size this element needs to display correctly.
	/// </summary>
	/// <remarks>
	/// This usually indicates the space absolutely sized children in <see cref="ILayoutContainer"/>s occupy.
	/// </remarks>
	Size2<float> RequiredSize { get; }
}
