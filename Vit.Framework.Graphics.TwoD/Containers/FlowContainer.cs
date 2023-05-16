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

	RelativeAxes2<float> flowOrigin;
	/// <summary>
	/// What point the flow elements are aligned to.
	/// </summary>
	public required RelativeAxes2<float> FlowOrigin {
		get => flowOrigin;
		set {
			if ( flowOrigin == value )
				return;

			flowOrigin = value;
			InvalidateLayout();
		}
	}

	FlowDirection flowDirection;
	public required FlowDirection FlowDirection {
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
		if ( Children.Count == 0 )
			return;

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

		var contentFlowSize = flowDirection.ToFlow( ContentSize );
		var flowOrigin = FlowOrigin.ToFlow( flowDirection, ContentSize );
		flowOrigin.Flow /= contentFlowSize.Flow;
		flowOrigin.Cross /= contentFlowSize.Cross;

		var positionOffset = flowDirection.MakePositionOffsetBySizeAxes<float>();

		void finalizeSpan () {
			crossPosition += tryCollapse( previousCrossMargin, crossStartMargin );

			var remainingFlow = contentFlowSize.Flow - (spanSize.Flow - flowPadding.Flow) - tryCollapse( previousFlowMargin, flowPadding.FlowEnd );
			foreach ( var (i, flow) in spanElements ) {
				var childSize = flowDirection.ToFlow( i.Size );
				i.Position = flowDirection.FromFlow( new FlowPoint2<float> {
					Flow = flow + remainingFlow * flowOrigin.Flow + childSize.Flow * positionOffset.Flow,
					Cross = crossPosition
				}, size );
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

		var coversBothDirections = flowDirection.GetCoveredDirections() == LayoutDirection.Both;
		foreach ( var (i, param) in LayoutChildren ) {
			var childSize = param.Size.GetSize( flowDirection, availableSpanSpace ).Contain( i.RequiredSize );
			var childFlowSize = flowDirection.ToFlow( childSize );

			i.Size = childSize;
			var margin = flowDirection.ToFlow( param.Margins );

			var flowStartMargin = tryCollapse( previousFlowMargin, margin.FlowStart );
			var flowEndMargin = tryCollapse( margin.FlowEnd, flowPadding.FlowEnd );

			if ( coversBothDirections && spanElements.Any() && spanSize.Flow + flowStartMargin + childFlowSize.Flow + flowEndMargin > availableSpanFlowSpace.Flow ) {
				finalizeSpan();
				flowStartMargin = tryCollapse( previousFlowMargin, margin.FlowStart );
			}

			spanSize.Flow += flowStartMargin;
			spanElements.Add((i, spanSize.Flow));

			spanSize.Cross = float.Max( spanSize.Cross, childFlowSize.Cross );
			crossEndMargin = float.Max( crossEndMargin, margin.CrossEnd );
			crossStartMargin = float.Max( crossStartMargin, margin.CrossStart );
			spanSize.Flow += childFlowSize.Flow;
			previousFlowMargin = margin.FlowEnd;

			if ( coversBothDirections && spanSize.Flow > availableSpanFlowSpace.Flow )
				finalizeSpan();
		}

		if ( spanElements.Any() )
			finalizeSpan();

		// finalize spans
		var remainingCross = contentFlowSize.Cross - (crossPosition - flowPadding.Cross) - tryCollapse( previousCrossMargin, flowPadding.CrossEnd );
		foreach ( var i in Children ) {
			var childSize = flowDirection.ToFlow( i.Size );
			var flowPosition = flowDirection.ToFlow( i.Position, size );
			i.Position = flowDirection.FromFlow( flowPosition with {
				Cross = flowPosition.Cross + remainingCross * flowOrigin.Cross + childSize.Cross * positionOffset.Cross
			}, size );
		}
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