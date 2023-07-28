﻿using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.UI.Layout;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class DrawableLayoutContainer : DrawableLayoutContainer<IDrawableLayoutElement> { }
public class DrawableLayoutContainer<T> : DrawableLayoutContainer<T, LayoutParams> where T : IDrawableLayoutElement {
	protected override void PerformLayout () {
		var size = ContentSize;
		var offset = new Vector2<float>( Padding.Left, Padding.Bottom );

		foreach ( var (i, param) in LayoutChildren ) {
			i.Size = param.Size.GetSize( size ).Contain( i.RequiredSize );

			var origin = i.Size * param.Origin;
			var anchor = size * param.Anchor;

			i.Position = (anchor - origin + offset).FromOrigin();
		}
	}

	protected override Size2<float> PerformAbsoluteLayout () {
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