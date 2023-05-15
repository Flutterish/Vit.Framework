using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class FlowContainer<T> : LayoutContainer<T, FlowParams> where T : class, ILayoutElement {
	bool collapseMargins = true;
	public bool CollapseMargins {
		get => collapseMargins;
		set {
			if ( collapseMargins == value )
				return;

			collapseMargins = value;
			InvalidateLayout();
		}
	}

	Axes2<float> flowOrigin;
	public Axes2<float> FlowOrigin {
		get => flowOrigin;
		set {
			if ( flowOrigin == value )
				return;

			flowOrigin = value;
			InvalidateLayout();
		}
	}

	FlowDirection flowDirection = FlowDirection.HorizontalThenVertical;
	public FlowDirection FlowDirection {
		get => flowDirection;
		set {
			if ( flowDirection == value )
				return;

			flowDirection = value;
			InvalidateLayout();
		}
	}

	protected override void PerformLayout () {
		
	}
}

public enum FlowDirection {
	HorizontalThenVertical,
	VerticalThenHorizontal,
	Horizontal,
	Vertical
}

public struct FlowParams {
	/// <summary>
	/// Margins outside the element provide spacing between elements.
	/// </summary>
	/// <remarks>
	/// Margins might collapse. For example, if the margins between 2 elements (including <see cref="ILayoutContainer.Padding"/>) are 10 and 20, the effective margin will be 20 or 30, depending on the container settings. <br/>
	/// Margins may be negative. This indicates that neighboring margins (including <see cref="ILayoutContainer.Padding"/>) should be shrunk.
	/// </remarks>
	public Spacing<float> Margins;
}