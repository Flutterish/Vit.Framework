using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Layout;

public abstract class FlowingLayoutContainer<T, TParam, TChildArgs> : LayoutContainer<T, TParam> where T : ILayoutElement where TParam : struct where TChildArgs : struct {
	RelativeAxes2<float> flowOrigin;
	/// <summary>
	/// What point the flow elements are aligned to when there is unused space in a span.
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
	/// How elements are aligned along the cross axis of a span.
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

	Justification spanAlignment;
	/// <summary>
	/// How spans are justified along the cross axis.
	/// </summary>
	public Justification SpanJustification {
		get => spanAlignment;
		set {
			if ( spanAlignment == value )
				return;

			spanAlignment = value;
			InvalidateLayout();
		}
	}

	protected sealed override Size2<float> PerformAbsoluteLayout () {
		CalculateLayoutConstants();
		return Size2<float>.Zero;
	}

	FlowSize2<float> contentSize;
	FlowAxes2<float> flowOriginAxes;
	List<(int start, int length, FlowSize2<float> size)> spans = new();
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
		var (offset, gap) = SpanJustification.GetOffsets( spans.Count, remaining, contentAlignment: flowOriginAxes.Cross );

		foreach ( var span in spans ) {
			foreach ( ref var i in layout.AsSpan( span.start, span.length ) ) {
				i.Position.Cross += offset;
			}
			offset += gap;
		}

		foreach ( ref var i in layout.AsSpan() ) {
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

		spans.Clear();
	}

	protected virtual void CalculateLayoutConstants () { }

	/// <summary>
	/// Finalizes a span by aligning elements on it. 
	/// This assumes that the child layout is such that the flow position monotonically increases and the cross position equals the start of the span.
	/// </summary>
	/// <param name="children">The children that consitute the span.</param>
	/// <param name="spanSize">The size of the span.</param>
	protected void FinalizeSpan ( SpanSlice<ChildLayout> children, FlowSize2<float> spanSize ) {
		spans.Add(( children.Start, children.Length, spanSize ));
		var remaining = contentSize.Flow - spanSize.Flow;
		var (offset, gap) = ItemJustification.GetOffsets( children.Length, remaining, contentAlignment: flowOriginAxes.Flow );

		foreach ( ref var i in children ) {
			i.Position.Flow += offset;
			offset += gap;
		}

		if ( ItemAlignment == Alignment.Stretch ) {
			foreach ( ref var i in children ) {
				i.Size.Cross = spanSize.Cross;
			}
		}
		else if ( ItemAlignment != Alignment.Start ) {
			offset = itemAlignment switch {
				Alignment.Center => 0.5f,
				Alignment.End or _ => 1f
			};

			foreach ( ref var i in children ) {
				i.Position.Cross += (spanSize.Cross - i.Size.Cross) * offset;
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
	/// You should call <see cref="FinalizeSpan(Span{ChildLayout}, float)"/> for each span you create.
	/// </remarks>
	/// <param name="context">The context of the layout.</param>
	/// <returns>The total cross size of the layout.</returns>
	protected abstract float PerformLayout ( LayoutContext context );

	protected struct ChildLayout {
		public FlowSize2<float> Size;
		public FlowPoint2<float> Position;
	}

	protected ref struct LayoutContext {
		/// <summary>
		/// Child arguments.
		/// </summary>
		public required ReadOnlySpan<TChildArgs> Children;
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
	/// Use <see cref="FlowingLayoutContainer{T, TParam, TChildArgs}.ContentAlignment"/>.
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

public enum Alignment {
	/// <summary>
	/// Align the start of the element with the start of the cross axis of a span.
	/// </summary>
	Start,
	/// <summary>
	/// Align the end of the element with the end of the cross axis of a span.
	/// </summary>
	End,
	/// <summary>
	/// Align the center of the element with the center of the cross axis of a span.
	/// </summary>
	Center,
	/// <summary>
	/// Stretch elements so that they fill the whole cross axis of a span. This ignores min/max size.
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
}