using Vit.Framework.Memory;
using Vit.Framework.TwoD.Layout;

namespace Vit.Framework.TwoD.UI.Layout;
// TODO grid, subgrid, masonry grid
public abstract class FlowingLayoutContainer<T, TParam, TChildArgs> : ParametrizedLayoutContainer<T, TParam> where T : UIComponent where TParam : struct where TChildArgs : struct {
	RelativeAxes2<float> flowOrigin = Anchor.TopLeft;
	/// <summary>
	/// What point the flow elements are aligned to when there is unused space in a line.
	/// </summary>
	public RelativeAxes2<float> ContentAlignment {
		get => flowOrigin;
		set {
			if ( flowOrigin == value )
				return;

			flowOrigin = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	FlowDirection flowDirection = FlowDirection.Right;
	/// <summary>
	/// Defines in which direction the content is laid out.
	/// Additionaly you can specify the direction in which wrapping occurs, or not wrap at all.
	/// </summary>
	public FlowDirection FlowDirection {
		get => flowDirection;
		set {
			if ( flowDirection == value )
				return;

			flowDirection = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	Justification justification;
	/// <summary>
	/// How elements along the flow axis are justified.
	/// </summary>
	public Justification ItemJustification {
		get => justification;
		set {
			if ( justification == value )
				return;

			justification = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	Alignment itemAlignment;
	/// <summary>
	/// How elements are aligned along the cross axis of a line.
	/// </summary>
	public Alignment ItemAlignment {
		get => itemAlignment;
		set {
			if ( itemAlignment == value )
				return;

			itemAlignment = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	LineJustification lineJustification = LineJustification.Stretch;
	/// <summary>
	/// How lines are justified along the cross axis.
	/// </summary>
	public LineJustification LineJustification {
		get => lineJustification;
		set {
			if ( lineJustification == value )
				return;

			lineJustification = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}

	FlowSize2<float> contentSize;
	FlowAxes2<float> flowOriginAxes;
	List<(int start, int length, FlowSize2<float> size)> lines = new();
	protected sealed override void PerformLayout () {
		if ( !Children.Any() )
			return;

		CalculateLayoutConstants();
		using var children = new RentedArray<TChildArgs>( Children.Count );
		using var layout = new RentedArray<ChildLayout>( Children.Count );
		for ( int i = 0; i < Children.Count; i++ ) {
			var child = Children[i];
			children[i] = GetChildArgs( child, GetLayoutParameters( child ) );
		}

		contentSize = flowDirection.ToFlow( ContentSize );
		var axes = flowOrigin.ToFlow( flowDirection, ContentSize );
		flowOriginAxes = new() {
			Flow = axes.Flow / contentSize.Flow,
			Cross = axes.Cross / contentSize.Cross
		};

		var crossSize = PerformLayout( new() {
			Children = children,
			Layout = layout,
			Padding = flowDirection.ToFlow( Padding ),
			Size = flowDirection.ToFlow( Size ),
			ContentSize = contentSize
		} );

		var sizeOffset = flowDirection.MakePositionOffsetBySizeAxes<float>();
		var remaining = contentSize.Cross - crossSize;

		var (offset, gap) = LineJustification.GetOffsets( lines.Count, remaining, contentAlignment: flowOriginAxes.Cross );
		foreach ( var line in lines ) {
			var lineLayouts = new SpanSlice<ChildLayout> { Source = layout.AsSpan(), Start = line.start, Length = line.length };
			var lineChildren = new SpanSlice<TChildArgs> { Source = children.AsSpan(), Start = line.start, Length = line.length };
			var lineSize = line.size;
			if ( lineJustification == LineJustification.Stretch ) lineSize.Cross += gap;
			foreach ( ref var i in layout.AsSpan( line.start, line.length ) ) {
				i.Position.Cross += offset;
			}
			offset += gap;

			FinalizeLine( lineChildren, lineLayouts, lineSize );
			alignLineElements( lineLayouts, lineSize );
		}

		foreach ( ref var i in layout ) {
			i.Position.Cross += i.Size.Cross * sizeOffset.Cross;
			i.Position.Flow += i.Size.Flow * sizeOffset.Flow;
		}

		var size = FlowDirection.ToFlow( Size );
		for ( int i = 0; i < Children.Count; i++ ) {
			var child = Children[i];
			var childLayout = layout[i];

			child.Size = flowDirection.FromFlow( childLayout.Size );
			child.Position = flowDirection.FromFlow( childLayout.Position, size );
		}

		lines.Clear();
		base.PerformLayout();
	}

	protected virtual void CalculateLayoutConstants () { }

	/// <summary>
	/// Submits a line for alignment purposes. After all lines have been submitted, 
	/// <see cref="FinalizeLine(SpanSlice{ChildLayout}, FlowSize2{float})"/> will be called for each line after aligning it.
	/// This assumes that the child layout is such that the flow position monotonically increases and the cross position equals the start of the line.
	/// </summary>
	/// <param name="children">The children that consitute the line.</param>
	/// <param name="lineSize">The size of the line.</param>
	protected void SubmitLine ( SpanSlice<ChildLayout> children, FlowSize2<float> lineSize ) {
		lines.Add( (children.Start, children.Length, lineSize) );
	}

	void alignLineElements ( SpanSlice<ChildLayout> children, FlowSize2<float> lineSize ) {
		var remaining = contentSize.Flow - lineSize.Flow;
		var (offset, gap) = ItemJustification.GetOffsets( children.Length, remaining, contentAlignment: flowOriginAxes.Flow );

		foreach ( ref var i in children ) {
			i.Position.Flow += offset;
			offset += gap;
		}

		if ( ItemAlignment == Alignment.Stretch ) {
			foreach ( ref var i in children ) {
				i.Size.Cross = lineSize.Cross;
			}
		}
		else if ( ItemAlignment != Alignment.Start ) {
			offset = itemAlignment switch {
				Alignment.Center => 0.5f,
				Alignment.End or _ => 1f
			};

			foreach ( ref var i in children ) {
				i.Position.Cross += (lineSize.Cross - i.Size.Cross) * offset;
			}
		}
	}

	/// <summary>
	/// Computes arguments for children, in flow space.
	/// </summary>
	protected abstract TChildArgs GetChildArgs ( T child, TParam param );
	/// <summary>
	/// Performs layout. You should not do any <strong>flow &lt;-&gt; cardinal</strong> conversions while perforrming layout.
	/// </summary>
	/// <remarks>
	/// You should call <see cref="SubmitLine(SpanSlice{ChildLayout}, FlowSize2{float})"/> for each line you create.
	/// </remarks>
	/// <param name="context">The context of the layout.</param>
	/// <returns>The total cross size of the layout.</returns>
	protected abstract float PerformLayout ( LayoutContext context );
	/// <summary>
	/// Finalizes layout on a line, such as sizing relatively sized elements across the cross axis.
	/// </summary>
	protected abstract void FinalizeLine ( SpanSlice<TChildArgs> children, SpanSlice<ChildLayout> layouts, FlowSize2<float> lineSize );

	protected struct ChildLayout {
		public FlowSize2<float> Size;
		public FlowPoint2<float> Position;
	}

	protected ref struct LayoutContext {
		/// <summary>
		/// Child arguments.
		/// </summary>
		public required Span<TChildArgs> Children;
		/// <summary>
		/// Computed layout for children. You need to fill this span with appropriate values when peforming layuout.
		/// </summary>
		public required Span<ChildLayout> Layout;
		/// <summary>
		/// Padding.
		/// </summary>
		public required FlowSpacing<float> Padding;
		/// <summary>
		/// Total available layout space.
		/// </summary>
		public required FlowSize2<float> Size;
		/// <summary>
		/// Available layout space after shrinking by padding.
		/// </summary>
		public required FlowSize2<float> ContentSize;
	}
}

public static class JustificationExtensions {
	public static (float offset, float gap) GetOffsets ( this Justification justification, int elementCount, float remainingSpace, float contentAlignment ) {
		return (elementCount > 1 && remainingSpace > 0 ? justification : Justification.ContentAlignment) switch {
			Justification.ContentAlignment => (remainingSpace * contentAlignment, 0f),
			Justification.SpaceBetween => (0, remainingSpace / (elementCount - 1)),
			Justification.SpaceAround => (remainingSpace / elementCount / 2, remainingSpace / elementCount),
			Justification.SpaceEvenly or _ => (remainingSpace / (elementCount + 1), remainingSpace / (elementCount + 1))
		};
	}

	public static (float offset, float gap) GetOffsets ( this LineJustification justification, int elementCount, float remainingSpace, float contentAlignment ) {
		return justification == LineJustification.Stretch
			? (0, remainingSpace / elementCount)
			: ((Justification)(int)justification).GetOffsets( elementCount, remainingSpace, contentAlignment );
	}
}