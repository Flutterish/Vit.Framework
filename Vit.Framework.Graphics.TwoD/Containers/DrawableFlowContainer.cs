﻿using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.UI.Layout;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class DrawableFlowContainer : DrawableFlowContainer<IDrawableLayoutElement> { }
public class DrawableFlowContainer<T> : DrawableFlowingLayoutContainer<T, FlowParams, DrawableFlowContainer<T>.ChildArgs> where T : IDrawableLayoutElement {
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
		var (flow, cross) = param.Size.ToFlow( FlowDirection );
		var required = FlowDirection.ToFlow( child.RequiredSize );

		return new() {
			FlowSize = flow.GetValue( contentFlowSize.Flow, min: required.Flow ),
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
		FlowSize2<float> lineSize = FlowSize2<float>.Zero;
		
		float previousFlowMargin = flowPadding.FlowStart;
		float previousCrossMargin = flowPadding.CrossStart;
		float crossStartMargin = 0;
		float crossEndMargin = 0;

		SpanSlice<ChildLayout> line = new() { Source = context.Layout };
		void finalizeLine ( ref SpanSlice<ChildLayout> layouts, Span<ChildArgs> args ) {
			crossPosition += getMargin( previousCrossMargin, crossStartMargin );

			int index = layouts.Start;
			foreach ( ref var i in layouts ) {
				var child = args[index++];

				i.Position.Cross = crossPosition;
			}
			SubmitLine( layouts, new() {
				Flow = lineSize.Flow - flowPadding.Flow + getMargin( previousFlowMargin, flowPadding.FlowEnd ),
				Cross = lineSize.Cross
			} );

			crossPosition += lineSize.Cross;
			lineSize = FlowSize2<float>.Zero;
			
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

			var childFlowSize = new FlowSize2<float>( child.FlowSize, child.CrossSize.GetValue( 0, min: child.RequiredCrossSize ) );

			layout.Size = childFlowSize;
			var margin = child.Margins;

			var flowStartMargin = getMargin( previousFlowMargin, margin.FlowStart );
			var flowEndMargin = getMargin( margin.FlowEnd, flowPadding.FlowEnd );

			if ( coversBothDirections && line.Length != 0 && lineSize.Flow + flowStartMargin + childFlowSize.Flow + flowEndMargin > context.Size.Flow ) {
				finalizeLine( ref line, context.Children );
				flowStartMargin = getMargin( previousFlowMargin, margin.FlowStart );
			}

			lineSize.Flow += flowStartMargin;
			line.Length++;
			layout.Position.Flow = lineSize.Flow;

			lineSize.Cross = float.Max( lineSize.Cross, childFlowSize.Cross );
			crossEndMargin = float.Max( crossEndMargin, margin.CrossEnd );
			crossStartMargin = float.Max( crossStartMargin, margin.CrossStart ); // TODO this depends on cross position/size
			lineSize.Flow += childFlowSize.Flow;
			previousFlowMargin = margin.FlowEnd;

			if ( coversBothDirections && lineSize.Flow > context.Size.Flow )
				finalizeLine( ref line, context.Children );
		}

		if ( line.Length != 0 )
			finalizeLine( ref line, context.Children );

		return crossPosition - flowPadding.Cross + getMargin( previousCrossMargin, flowPadding.CrossEnd );
	}

	protected override void FinalizeLine ( SpanSlice<ChildArgs> children, SpanSlice<ChildLayout> layouts, FlowSize2<float> lineSize ) {
		for ( int i = 0; i < children.Length; i++ ) {
			var arg = children[i];
			ref var layout = ref layouts[i];

			layout.Size.Cross = arg.CrossSize.GetValue( lineSize.Cross, min: arg.RequiredCrossSize );
		}
	}

	public struct ChildArgs {
		public float FlowSize;
		public BoundedLayoutUnit<float> CrossSize;
		public float RequiredCrossSize;
		public FlowSpacing<float> Margins;
	}
}