using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class FlowContainer<T> : LayoutContainer<T, FlowParams> where T : ILayoutElement {
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

	RelativeAxes2<float> flowOrigin = Anchor.TopLeft;
	public RelativeAxes2<float> FlowOrigin {
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



	List<(T element, float flow)> spanElements = new();
	protected override void PerformLayout () {
		var size = flowDirection.ToFlow( new Size2<float>( ContentSize.Width + Padding.Horizontal, ContentSize.Height + Padding.Vertical ) );
		var availableSpanFlowSpace = size with { Cross = 0 };
		var availableSpanSpace = FlowDirection.FromFlow( availableSpanFlowSpace );
		var flowPadding = flowDirection.ToFlow( Padding );

		float crossPosition = 0;
		FlowSize2<float> spanSize = FlowSize2<float>.Zero;
		float previousFlowMargin = flowPadding.FlowStart;
		float previousCrossMargin = flowPadding.CrossStart;
		float crossStartMargin = 0;
		float crossEndMargin = 0;

		void finalizeSpan () {
			crossPosition += tryCollapse( previousCrossMargin, crossStartMargin );

			foreach ( var (i, flow) in spanElements ) {
				i.Position = flowDirection.FromFlow( new FlowPoint2<float> {
					Flow = flow,
					Cross = crossPosition
				} );
			}

			crossPosition += spanSize.Cross;
			spanSize = FlowSize2<float>.Zero;
			previousFlowMargin = flowPadding.FlowStart;
			previousCrossMargin = crossEndMargin;
			crossEndMargin = 0;
			crossStartMargin = 0;
			spanElements.Clear();
		}

		float tryCollapse ( float end, float start ) {
			if ( !collapseMargins )
				return end + start;

			return float.Max( end, start );
		}
		
		foreach ( var (i, param) in LayoutChildren ) {
			var childSize = param.Size.GetSize( flowDirection, availableSpanSpace ).Contain( i.RequiredSize );
			var childFlowSize = flowDirection.ToFlow( childSize );

			i.Size = childSize;
			var margin = flowDirection.ToFlow( param.Margins );

			var flowStartMargin = tryCollapse( previousFlowMargin, margin.FlowStart );
			spanSize.Flow += flowStartMargin;

			var flowEndMargin = tryCollapse( margin.FlowEnd, flowPadding.FlowEnd );

			if ( spanElements.Any() && spanSize.Flow + flowEndMargin + childFlowSize.Flow > availableSpanFlowSpace.Flow ) {
				finalizeSpan();
				flowStartMargin = tryCollapse( previousFlowMargin, margin.FlowStart );
				spanSize.Flow += flowStartMargin;
			}

			spanElements.Add((i, spanSize.Flow));

			spanSize.Cross = float.Max( spanSize.Cross, childFlowSize.Cross );
			crossEndMargin = float.Max( crossEndMargin, margin.CrossEnd );
			crossStartMargin = float.Max( crossStartMargin, margin.CrossStart );
			spanSize.Flow += childFlowSize.Flow;
			previousFlowMargin = margin.FlowEnd;

			if ( spanSize.Flow > availableSpanFlowSpace.Flow )
				finalizeSpan();
		}

		finalizeSpan();
	}

	protected override Size2<float> PerformAbsoluteLayout () {
		return Size2<float>.Zero;
	}
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
	/// <summary>
	/// Size of the element.
	/// </summary>
	public LayoutSize<float> Size;
}