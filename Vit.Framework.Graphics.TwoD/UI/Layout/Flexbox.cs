using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.UI.Layout;

public class Flexbox : Flexbox<UIComponent> { }
public class Flexbox<T> : FlowingLayoutContainer<T, FlexboxParams, Flexbox<T>.ChildArgs> where T : UIComponent {
	Size2<float> gap;
	/// <summary>
	/// Gap between elements.
	/// </summary>
	public Size2<float> Gap {
		get => gap;
		set {
			if ( gap == value )
				return;

			gap = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	FlowSize2<float> contentFlowSize;
	FlowSize2<float> gapSize;
	protected override void CalculateLayoutConstants () {
		contentFlowSize = FlowDirection.ToFlow( ContentSize );
		gapSize = FlowDirection.ToFlow( Gap );
	}

	protected override ChildArgs GetChildArgs ( T child, FlexboxParams param ) {
		var (flow, cross) = param.Size.ToFlow( FlowDirection );
		var required = FlowDirection.ToFlow( child.RequiredSize );

		var max = float.Max( flow.Max?.GetValue( contentFlowSize.Flow ) ?? float.PositiveInfinity, required.Flow );
		return new() {
			FlowSize = flow.GetValue( contentFlowSize.Flow, min: required.Flow ),
			CrossSize = cross,
			RequiredCrossSize = required.Cross,

			Grow = param.Grow,
			Shrink = param.Shrink ?? 1,

			MaxFlowSize = max,
			MinFlowSize = float.Min( float.Max( flow.Min?.GetValue( contentFlowSize.Flow ) ?? 0, required.Flow ), max )
		};
	}

	void flexGrow ( Span<ChildLayout> layouts, Span<ChildArgs> children, float flowSize ) {
		foreach ( ref var i in children ) {
			if ( i.Grow <= 0 )
				i.IsFrozen = true;
		}

		float getGrowSum ( Span<ChildLayout> layouts, Span<ChildArgs> children ) {
			var sum = 0f;
			for ( int i = 0; i < children.Length; i++ ) {
				ref var child = ref children[i];
				if ( child.IsFrozen )
					continue;

				var layout = layouts[i];
				if ( layout.Size.Flow >= child.MaxFlowSize ) {
					child.IsFrozen = true;
					continue;
				}

				sum += child.Grow;
			}

			return sum;
		}

		float growSum;
		float remainingSpace;
		while (
			(remainingSpace = flowSize - (layouts[^1].Position.Flow + layouts[^1].Size.Flow - layouts[0].Position.Flow)) > 0
			&& (growSum = getGrowSum( layouts, children )) != 0
		) {
			var maxApply = 1f; // TODO limit grow when < 1

			for ( int i = 0; i < children.Length; i++ ) {
				var child = children[i];
				if ( child.IsFrozen )
					continue;

				var layout = layouts[i];
				var maxGrow = float.Min( child.MaxFlowSize - layout.Size.Flow, remainingSpace );

				maxApply = float.Min( maxApply, maxGrow / remainingSpace * growSum / child.Grow );
			}

			float offset = 0;
			for ( int i = 0; i < children.Length; i++ ) {
				var child = children[i];
				ref var layout = ref layouts[i];

				layout.Position.Flow += offset;

				if ( !child.IsFrozen ) {
					var grow = remainingSpace * maxApply * child.Grow / growSum;
					layout.Size.Flow += grow;
					offset += grow;
				}
			}
		}
	}

	void flexShrink ( Span<ChildLayout> layouts, Span<ChildArgs> children, float flowSize ) {
		foreach ( ref var i in children ) {
			if ( i.Shrink <= 0 )
				i.IsFrozen = true;
		}

		float getShrinkSum ( Span<ChildLayout> layouts, Span<ChildArgs> children ) {
			var sum = 0f;
			for ( int i = 0; i < children.Length; i++ ) {
				ref var child = ref children[i];
				if ( child.IsFrozen )
					continue;

				var layout = layouts[i];
				if ( layout.Size.Flow <= child.MinFlowSize ) {
					child.IsFrozen = true;
					continue;
				}

				sum += child.Shrink * child.FlowSize;
			}

			return sum;
		}

		float shrinkSum;
		float remainingNegativeSpace;
		while (
			(remainingNegativeSpace = -(flowSize - (layouts[^1].Position.Flow + layouts[^1].Size.Flow - layouts[0].Position.Flow))) > 0
			&& (shrinkSum = getShrinkSum( layouts, children )) != 0
		) {
			var maxApply = 1f;

			for ( int i = 0; i < children.Length; i++ ) {
				var child = children[i];
				if ( child.IsFrozen )
					continue;

				var layout = layouts[i];
				var maxShrink = float.Min( layout.Size.Flow - child.MinFlowSize, remainingNegativeSpace );

				maxApply = float.Min( maxApply, maxShrink * shrinkSum / child.Shrink / child.FlowSize / remainingNegativeSpace );
			}

			float offset = 0;
			for ( int i = 0; i < children.Length; i++ ) {
				var child = children[i];
				ref var layout = ref layouts[i];

				layout.Position.Flow -= offset;

				if ( !child.IsFrozen ) {
					var shrink = remainingNegativeSpace * maxApply * child.Shrink * child.FlowSize / shrinkSum;
					layout.Size.Flow -= shrink;
					offset += shrink;
				}
			}
		}
	}

	protected override float PerformLayout ( LayoutContext context ) {
		var padding = context.Padding;

		var contentSize = context.ContentSize;
		float crossPosition = padding.CrossStart;
		FlowSize2<float> lineSize = FlowSize2<float>.Zero;
		bool isFirstLine = true;
		void finalizeLine ( ref SpanSlice<ChildLayout> line, Span<ChildArgs> args ) {
			if ( !isFirstLine ) {
				crossPosition += gapSize.Cross;
			}
			isFirstLine = false;

			int index = line.Start;
			foreach ( ref var i in line ) {
				var child = args[index++];

				i.Position.Cross = crossPosition;
				i.Position.Flow += padding.FlowStart;
			}

			args = args.Slice( line.Start, line.Length );
			var remainingSpace = contentSize.Flow - (line[^1].Position.Flow + line[^1].Size.Flow - line[0].Position.Flow);
			if ( remainingSpace > 0 ) {
				flexGrow( line, args, contentSize.Flow );
				lineSize.Flow = line[^1].Position.Flow + line[^1].Size.Flow - line[0].Position.Flow;
			}
			else if ( remainingSpace < 0 ) {
				flexShrink( line, args, contentSize.Flow );
				lineSize.Flow = line[^1].Position.Flow + line[^1].Size.Flow - line[0].Position.Flow;
			}

			SubmitLine( line, lineSize );

			crossPosition += lineSize.Cross;
			lineSize = FlowSize2<float>.Zero;
			line.Start += line.Length;
			line.Length = 0;
		}

		var coversBothDirections = FlowDirection.GetCoveredDirections() == LayoutDirection.Both;
		var layouts = context.Layout;
		var children = context.Children;
		SpanSlice<ChildLayout> line = new() { Source = layouts };
		for ( int i = 0; i < children.Length; i++ ) {
			ref var layout = ref layouts[i];
			var child = children[i];

			var size = new FlowSize2<float>( child.FlowSize, child.CrossSize.GetValue( 0, min: child.RequiredCrossSize ) );
			if ( coversBothDirections && line.Length != 0 && lineSize.Flow + size.Flow > contentSize.Flow ) {
				finalizeLine( ref line, children );
			}

			layout.Size.Flow = size.Flow;
			if ( line.Length != 0 )
				lineSize.Flow += gapSize.Flow;
			layout.Position.Flow = lineSize.Flow;
			lineSize.Flow += size.Flow;
			lineSize.Cross = float.Max( lineSize.Cross, size.Cross );
			line.Length++;

			if ( coversBothDirections && lineSize.Flow > contentSize.Flow ) {
				finalizeLine( ref line, children );
			}
		}

		if ( line.Length != 0 )
			finalizeLine( ref line, children );

		return crossPosition - padding.CrossStart;
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

		public float Grow;
		public float Shrink;

		public float MaxFlowSize;
		public float MinFlowSize;

		public bool IsFrozen;
	}
}

public struct FlexboxParams {
	/// <summary>
	/// Size of the element.
	/// </summary>
	public SizeBounds2<float> Size;

	/// <summary>
	/// Weight which determines how much of the remaining space this element will consume.
	/// </summary>
	/// <remarks>
	/// This is expressed as a percentage (1 = 100%) of free space this element would like to consume. If below 1, this element will not consume all of the free space.
	/// </remarks>
	public float Grow;
	/// <summary>
	/// Weight which determines how much this element will shrink when there is not enough space.
	/// </summary>
	/// <remarks>
	/// Defaults to 1. This is proportional to the size of the element.
	/// </remarks>
	public float? Shrink;
}