using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Layout;

public abstract class FlowingLayoutContainer<T, TParam, TChildArgs> : LayoutContainer<T, TParam> where T : ILayoutElement where TParam : struct where TChildArgs : struct {
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

	protected sealed override Size2<float> PerformAbsoluteLayout () {
		return Size2<float>.Zero;
	}

	FlowSize2<float> contentSize;
	FlowAxes2<float> flowOriginAxes;
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
		var offset = remaining * flowOriginAxes.Cross;
		foreach ( ref var i in layout.AsSpan() ) {
			i.Position.Cross += offset + i.Size.Cross * sizeOffset.Cross;
			i.Position.Flow += i.Size.Flow * sizeOffset.Flow;
		}

		var size = FlowDirection.ToFlow( Size );
		for ( int i = 0; i < Children.Count; i++ ) {
			var child = Children[i];
			var childLayout = layout[i];

			child.Size = flowDirection.FromFlow( childLayout.Size );
			child.Position = flowDirection.FromFlow( childLayout.Position, size );
		}
	}

	protected virtual void CalculateLayoutConstants () { }

	protected void FinalizeSpan ( Span<ChildLayout> children, float spanFlowSize ) {
		var remaining = contentSize.Flow - spanFlowSize;
		var offset = remaining * flowOriginAxes.Flow;
		foreach ( ref var i in children ) {
			i.Position.Flow += offset;
		}
	}

	/// <summary>
	/// Computes arguments for children, in flow space.
	/// </summary>
	protected abstract TChildArgs GetChildArgs ( T child, TParam param );
	/// <summary>
	/// Performs layout. You should not do any <strong>flow &lt;-&gt; cardinal</strong> conversions while perforrming layout, other than converting custom arguments to flow space at startup.
	/// </summary>
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