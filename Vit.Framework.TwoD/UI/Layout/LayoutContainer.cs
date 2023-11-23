using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;

namespace Vit.Framework.TwoD.UI.Layout;

public class LayoutContainer : LayoutContainer<UIComponent> { }
public class LayoutContainer<T> : ParametrizedLayoutContainer<T, LayoutParams> where T : UIComponent {
	protected override void PerformSelfLayout () {
		var size = ContentSize;
		var offset = new Vector2<float>( Padding.Left, Padding.Bottom );

		foreach ( var (i, param) in LayoutChildren ) {
			i.Size = param.Size.GetSize( size ).Contain( i.RequiredSize );

			var origin = i.Size * param.Origin;
			var anchor = size * param.Anchor;

			i.Position = (anchor - origin + offset).FromOrigin();
		}
	}

	protected override void OnChildParameterUpdated ( T child, LayoutParams? previous, LayoutParams? current ) {
		var invalidation = LayoutInvalidations.Self;

		if ( AutoSizeDirection.HasFlag( LayoutDirection.Horizontal ) && previous?.Size.Width.Absolute != current?.Size.Width.Absolute )
			invalidation |= LayoutInvalidations.RequiredSize;
		else if ( AutoSizeDirection.HasFlag( LayoutDirection.Vertical ) && previous?.Size.Height.Absolute != current?.Size.Height.Absolute )
			invalidation |= LayoutInvalidations.RequiredSize;

		InvalidateLayout( invalidation );
	}

	public override void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations ) {
		if ( invalidations.HasFlag( LayoutInvalidations.RequiredSize ) )
			InvalidateLayout( LayoutInvalidations.Children | LayoutInvalidations.Self | LayoutInvalidations.RequiredSize );
		else
			InvalidateLayout( LayoutInvalidations.Children );
	}

	protected override Size2<float> ComputeRequiredSize () {
		if ( AutoSizeDirection == LayoutDirection.None )
			return new Size2<float>( Padding.Horizontal, Padding.Vertical );

		var result = new Size2<float>();

		var size = Size2<float>.Zero;
		foreach ( var (i, param) in LayoutChildren ) {
			var childSize = param.Size.GetSize( size ).Contain( i.RequiredSize );

			if ( AutoSizeDirection.HasFlag( LayoutDirection.Horizontal ) && !param.IgnoreAutosize.HasFlag( LayoutDirection.Horizontal ) ) 
				result.Width = float.Max( result.Width, childSize.Width );
			if ( AutoSizeDirection.HasFlag( LayoutDirection.Vertical ) && !param.IgnoreAutosize.HasFlag( LayoutDirection.Vertical ) ) 
				result.Height = float.Max( result.Height, childSize.Height );
		}

		return new() {
			Width = result.Width + Padding.Horizontal,
			Height = result.Height + Padding.Vertical
		};
	}
}

public struct LayoutParams : IInterpolatable<LayoutParams, float> {
	/// <summary>
	/// Size of the element.
	/// </summary>
	public RelativeSize2<float> Size;
	/// <summary>
	/// Point on the element which will be placed on <see cref="Anchor"/>.
	/// </summary>
	public RelativeAxes2<float> Origin;
	/// <summary>
	/// Point on the parent on which <see cref="Origin"/> will be placed.
	/// </summary>
	public RelativeAxes2<float> Anchor;

	/// <summary>
	/// The directions to ignore autosize for this element.
	/// </summary>
	public LayoutDirection IgnoreAutosize;

	public LayoutParams Lerp ( LayoutParams goal, float time ) {
		return new() {
			Size = Size.Lerp( goal.Size, time ),
			Origin = Origin.Lerp( goal.Origin, time ),
			Anchor = Anchor.Lerp( goal.Anchor, time ),
			IgnoreAutosize = goal.IgnoreAutosize
		};
	}
}
