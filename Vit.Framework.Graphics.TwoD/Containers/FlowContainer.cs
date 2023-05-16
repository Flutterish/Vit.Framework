﻿using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class FlowContainer<T> : FlowingLayoutContainer<T, FlowParams, FlowContainer<T>.ChildArgs> where T : ILayoutElement {
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

	FlowSize2<float> contentFlowSize;
	protected override void CalculateLayoutConstants () {
		contentFlowSize = FlowDirection.ToFlow( ContentSize );
	}

	protected override ChildArgs GetChildArgs ( T child, FlowParams param ) {
		var (flow, cross) = param.Size.GetFlowLayoutUnits( FlowDirection );
		var required = FlowDirection.ToFlow( child.RequiredSize );

		return new() {
			FlowSize = float.Max( flow.GetValue( contentFlowSize.Flow ), required.Flow ),
			CrossSize = cross,
			RequiredCrossSize = required.Cross,
			Margins = FlowDirection.ToFlow( param.Margins )
		};
	}

	float getMargin ( float end, float start ) {
		var max = float.Max( end, start );
		var min = float.Min( end, start );

		if ( min < 0 )
			return float.Max( 0, max + min );

		if ( !collapseMargins )
			return end + start;

		return max;
	}

	protected override float PerformLayout ( LayoutContext context ) {
		var flowPadding = context.Padding;

		float crossPosition = 0;
		FlowSize2<float> spanSize = FlowSize2<float>.Zero;
		
		float previousFlowMargin = flowPadding.FlowStart;
		float previousCrossMargin = flowPadding.CrossStart;
		float crossStartMargin = 0;
		float crossEndMargin = 0;

		SpanSlice<ChildLayout> span = new() { Source = context.Layout };
		void finalizeSpan ( ref SpanSlice<ChildLayout> layouts ) {
			crossPosition += getMargin( previousCrossMargin, crossStartMargin );

			FinalizeSpan( layouts, spanSize.Flow - flowPadding.Flow + getMargin( previousFlowMargin, flowPadding.FlowEnd ) );
			foreach ( ref var i in layouts ) {
				i.Position.Cross = crossPosition;
			}

			crossPosition += spanSize.Cross;
			spanSize = FlowSize2<float>.Zero;
			
			previousFlowMargin = flowPadding.FlowStart;
			previousCrossMargin = crossEndMargin;
			crossEndMargin = 0;
			crossStartMargin = 0;

			layouts.Start += layouts.Length;
			layouts.Length = 0;
		}

		var coversBothDirections = FlowDirection.GetCoveredDirections() == LayoutDirection.Both;
		for ( int i = 0; i < context.Children.Length; i++ ) {
			ref var layout = ref context.Layout[i];
			var child = context.Children[i];

			var childFlowSize = new FlowSize2<float>( child.FlowSize, float.Max( child.CrossSize.GetValue( 0 ), child.RequiredCrossSize ) );

			layout.Size = childFlowSize;
			var margin = child.Margins;

			var flowStartMargin = getMargin( previousFlowMargin, margin.FlowStart );
			var flowEndMargin = getMargin( margin.FlowEnd, flowPadding.FlowEnd );

			if ( coversBothDirections && span.Length != 0 && spanSize.Flow + flowStartMargin + childFlowSize.Flow + flowEndMargin > context.Size.Flow ) {
				finalizeSpan( ref span );
				flowStartMargin = getMargin( previousFlowMargin, margin.FlowStart );
			}

			spanSize.Flow += flowStartMargin;
			span.Length++;
			layout.Position.Flow = spanSize.Flow;

			spanSize.Cross = float.Max( spanSize.Cross, childFlowSize.Cross );
			crossEndMargin = float.Max( crossEndMargin, margin.CrossEnd );
			crossStartMargin = float.Max( crossStartMargin, margin.CrossStart );
			spanSize.Flow += childFlowSize.Flow;
			previousFlowMargin = margin.FlowEnd;

			if ( coversBothDirections && spanSize.Flow > context.Size.Flow )
				finalizeSpan( ref span );
		}

		if ( span.Length != 0 )
			finalizeSpan( ref span );

		return crossPosition - flowPadding.Cross + getMargin( previousCrossMargin, flowPadding.CrossEnd );
	}

	public struct ChildArgs {
		public float FlowSize;
		public LayoutUnit<float> CrossSize;
		public float RequiredCrossSize;
		public FlowSpacing<float> Margins;
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