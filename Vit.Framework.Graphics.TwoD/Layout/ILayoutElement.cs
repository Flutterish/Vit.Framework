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

	// TODO "required size"
}
