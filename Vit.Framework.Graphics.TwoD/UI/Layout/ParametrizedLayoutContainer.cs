using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI.Layout;

public abstract class ParametrizedLayoutContainer<T, TParam> : ParametrizedContainer<T, TParam> where T : UIComponent where TParam : struct {
	/// <summary>
	/// The size available to lay out child elements in, in local space.
	/// This accounts for <see cref="Padding"/>.
	/// </summary>
	public Size2<float> ContentSize => new( Size.Width - padding.Horizontal, Size.Height - padding.Vertical );

	Spacing<float> padding;
	/// <summary>
	/// Padding provides spacing between the container edges and elements. 
	/// It is in-set into the container, so that the container layout size does not change because of padding.
	/// </summary>
	/// <remarks>
	/// Padding may be negative in order to display content outside the actual container bounds.
	/// </remarks>
	public Spacing<float> Padding {
		get => padding;
		set {
			padding = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	LayoutDirection autoSizeDirection;
	/// <summary>
	/// Directions in which this layout container should calculate its size based on children.
	/// </summary>
	public LayoutDirection AutoSizeDirection {
		get => autoSizeDirection;
		set {
			if ( autoSizeDirection == value )
				return;

			autoSizeDirection = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}
}
