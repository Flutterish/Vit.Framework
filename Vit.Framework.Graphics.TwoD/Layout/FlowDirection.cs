using System.Numerics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD.Layout;

public enum FlowDirection {
	HorizontalThenVertical,
	VerticalThenHorizontal,
	Horizontal,
	Vertical
}

public static class FlowDirectionExtensions {
	public static LayoutDirection GetCoveredDirections ( this FlowDirection flow ) => flow switch {
		FlowDirection.Horizontal => LayoutDirection.Horizontal,
		FlowDirection.Vertical => LayoutDirection.Vertical,
		_ => LayoutDirection.Both
	};

	public static LayoutDirection GetFlowDirection ( this FlowDirection flow ) => flow switch {
		FlowDirection.Horizontal or FlowDirection.HorizontalThenVertical => LayoutDirection.Horizontal,
		_ => LayoutDirection.Vertical
	};

	public static LayoutDirection GetCrossDirection ( this FlowDirection flow ) => flow switch {
		FlowDirection.Horizontal or FlowDirection.HorizontalThenVertical => LayoutDirection.Vertical,
		_ => LayoutDirection.Horizontal
	};

	public static FlowSize2<T> ToFlow<T> ( this FlowDirection flow, Size2<T> size ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( size )
			: new( size.Height, size.Width );
	public static Size2<T> FromFlow<T> ( this FlowDirection flow, FlowSize2<T> size ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( size )
			: new( size.Cross, size.Flow );

	public static FlowVector2<T> ToFlow<T> ( this FlowDirection flow, Vector2<T> vector ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( vector )
			: new( vector.Y, vector.X );
	public static Vector2<T> FromFlow<T> ( this FlowDirection flow, FlowVector2<T> vector ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( vector )
			: new( vector.Cross, vector.Flow );

	public static FlowPoint2<T> ToFlow<T> ( this FlowDirection flow, Point2<T> point ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( point )
			: new( point.Y, point.X );
	public static Point2<T> FromFlow<T> ( this FlowDirection flow, FlowPoint2<T> point ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( point )
			: new( point.Cross, point.Flow );

	public static FlowSpacing<T> ToFlow<T> ( this FlowDirection flow, Spacing<T> spacing ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new() { FlowStart = spacing.Left, FlowEnd = spacing.Right, CrossStart = spacing.Bottom, CrossEnd = spacing.Top }
			: new() { FlowStart = spacing.Right, FlowEnd = spacing.Left, CrossStart = spacing.Top, CrossEnd = spacing.Bottom };
	public static Spacing<T> FromFlow<T> ( this FlowDirection flow, FlowSpacing<T> spacing ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new() { Left = spacing.FlowStart, Right = spacing.FlowEnd, Bottom = spacing.CrossStart, Top = spacing.CrossEnd }
			: new() { Left = spacing.FlowEnd, Right = spacing.FlowStart, Bottom = spacing.CrossEnd, Top = spacing.CrossStart };
}