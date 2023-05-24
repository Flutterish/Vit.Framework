using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Layout;

public abstract class FlowingLayoutContainer<T, TParam, TChildArgs> : LayoutContainer<T, TParam> where T : ILayoutElement where TParam : struct where TChildArgs : struct {
	RelativeAxes2<float> flowOrigin;
	/// <summary>
	/// What point the flow elements are aligned to when there is unused space in a line.
	/// </summary>
	public required RelativeAxes2<float> ContentAlignment {
		get => flowOrigin;
		set {
			if ( flowOrigin == value )
				return;

			flowOrigin = value;
			InvalidateLayout();
		}
	}

	FlowDirection flowDirection;
	/// <summary>
	/// Defines in which direction the content is laid out.
	/// Additionaly you can specify the direction in which wrapping occurs, or not wrap at all.
	/// </summary>
	public required FlowDirection FlowDirection {
		get => flowDirection;
		set {
			if ( flowDirection == value )
				return;

			flowDirection = value;
			InvalidateLayout();
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
			InvalidateLayout();
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
			InvalidateLayout();
		}
	}

	LineJustification lineJustification;
	/// <summary>
	/// How lines are justified along the cross axis.
	/// </summary>
	public LineJustification LineJustification {
		get => lineJustification;
		set {
			if ( lineJustification == value )
				return;

			lineJustification = value;
			InvalidateLayout();
		}
	}

	protected sealed override Size2<float> PerformAbsoluteLayout () {
		CalculateLayoutConstants();
		return Size2<float>.Zero;
	}

	FlowSize2<float> contentSize;
	FlowAxes2<float> flowOriginAxes;
	List<(int start, int length, FlowSize2<float> size)> lines = new();
	protected sealed override void PerformLayout () {
		if ( !Children.Any() )
			return;

		using var children = new RentedArray<TChildArgs>( Children.Count );
		using var layout = new RentedArray<ChildLayout>( Children.Count );
		for ( int i = 0; i < Children.Count; i++ ) {
			var child = Children[i];
			children[i] = GetChildArgs( child, GetLayoutParameters( child ) );
		}

		contentSize = flowDirection.ToFlow( ContentSize );
		var axes = flowOrigin.ToFlow( flowDirection, ContentSize);
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
			if ( lineJustification == LineJustification.Stretch ) {
				lineSize.Cross += gap;
			}
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

public enum Justification {
	/// <summary>
	/// Use ContentAlignment of the container.
	/// </summary>
	ContentAlignment,
	/// <summary>
	/// Put space between elements.
	/// </summary>
	SpaceBetween,
	/// <summary>
	/// Put space around element edges.
	/// </summary>
	SpaceAround,
	/// <summary>
	/// Put space between elements and container edges.
	/// </summary>
	SpaceEvenly
}


public enum LineJustification {
	/// <summary>
	/// Use ContentAlignment of the container.
	/// </summary>
	ContentAlignment = Justification.ContentAlignment,
	/// <summary>
	/// Put space between elements.
	/// </summary>
	SpaceBetween = Justification.SpaceBetween,
	/// <summary>
	/// Put space around element edges.
	/// </summary>
	SpaceAround = Justification.SpaceAround,
	/// <summary>
	/// Put space between elements and container edges.
	/// </summary>
	SpaceEvenly = Justification.SpaceEvenly,
	/// <summary>
	/// Stretch elements so that they fill the whole axis. This ignores max size.
	/// </summary>
	Stretch
}

public enum Alignment {
	/// <summary>
	/// Align the start of the element with the start of the cross axis of a line.
	/// </summary>
	Start,
	/// <summary>
	/// Align the end of the element with the end of the cross axis of a line.
	/// </summary>
	End,
	/// <summary>
	/// Align the center of the element with the center of the cross axis of a line.
	/// </summary>
	Center,
	/// <summary>
	/// Stretch elements so that they fill the whole cross axis of a line. This ignores max size.
	/// </summary>
	Stretch
}

public static class JustificationExtensions {
	public static (float offset, float gap) GetOffsets ( this Justification justification, int elementCount, float remainingSpace, float contentAlignment ) {
		return ((elementCount > 1 && remainingSpace > 0) ? justification : Justification.ContentAlignment) switch {
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