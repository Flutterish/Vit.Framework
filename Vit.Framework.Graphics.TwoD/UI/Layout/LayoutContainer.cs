﻿using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI.Layout;

public class LayoutContainer : LayoutContainer<UIComponent> { }
public class LayoutContainer<T> : ParametrizedLayoutContainer<T, LayoutParams> where T : UIComponent {
	protected override void PerformLayout () {
		var size = ContentSize;
		var offset = new Vector2<float>( Padding.Left, Padding.Bottom );

		foreach ( var (i, param) in LayoutChildren ) {
			i.Size = param.Size.GetSize( size ).Contain( i.RequiredSize );

			var origin = i.Size * param.Origin;
			var anchor = size * param.Anchor;

			i.Position = (anchor - origin + offset).FromOrigin();
		}

		base.PerformLayout();
	}

	protected override Size2<float> ComputeRequiredSize () {
		if ( AutoSizeDirection == LayoutDirection.None )
			return new Size2<float>( Padding.Horizontal, Padding.Vertical );

		var result = new Size2<float>();

		var size = Size2<float>.Zero;
		foreach ( var (i, param) in LayoutChildren ) {
			var childSize = param.Size.GetSize( size ).Contain( i.RequiredSize );

			var origin = childSize * param.Origin;
			var anchor = size * param.Anchor;

			var position = anchor - origin;

			if ( AutoSizeDirection.HasFlag( LayoutDirection.Horizontal ) ) {
				result.Width = float.Max( result.Width, position.X + childSize.Width );
			}
			if ( AutoSizeDirection.HasFlag( LayoutDirection.Vertical ) ) {
				result.Height = float.Max( result.Height, position.Y + childSize.Height );
			}
		}

		return new() {
			Width = result.Width + Padding.Horizontal,
			Height = result.Height + Padding.Vertical
		};
	}
}

public struct LayoutParams {
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
}