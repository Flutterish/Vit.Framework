using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

[Flags]
public enum FlowDirection {
	FlowLeft = 0b_0_00_00,
	FlowRight = 0b_0_00_01,
	FlowUp = 0b_0_00_10,
	FlowDown = 0b_0_00_11,

	HasCrossBit = 0b_1_00_00,
	CrossLeft = 0b_1_00_00,
	CrossRight = 0b_1_01_00,
	CrossUp = 0b_1_10_00,
	CrossDown = 0b_1_11_00,

	RightThenDown = FlowRight | CrossDown,
	RightThenUp = FlowRight | CrossUp,
	Right = FlowRight,

	LeftThenDown = FlowLeft | CrossDown,
	LeftThenUp = FlowLeft | CrossUp,
	Left = FlowLeft,

	DownThenRight = FlowDown | CrossRight,
	DownThenLeft = FlowDown | CrossLeft,
	Down = FlowDown,

	UpThenRight = FlowUp | CrossRight,
	UpThenLeft = FlowUp | CrossLeft,
	Up = FlowUp
}

public static class FlowDirectionExtensions {
	public static FlowAxes2<T> MakePositionOffsetBySizeAxes<T> ( this FlowDirection flow ) where T : INumber<T> {
		var primary = flow.GetPrimaryDirection();
		var secondary = flow.GetSecondaryDirection();

		return new() {
			Flow = primary is FlowDirection.Left or FlowDirection.Down ? T.One : T.Zero,
			Cross = secondary is FlowDirection.CrossLeft or FlowDirection.CrossDown ? T.One : T.Zero
		};
	}

	public static FlowDirection GetPrimaryDirection ( this FlowDirection flow ) => flow & (FlowDirection)0b_0_00_11;
	public static FlowDirection GetSecondaryDirection ( this FlowDirection flow ) => flow & (FlowDirection)0b_1_11_00;

	public static LayoutDirection GetCoveredDirections ( this FlowDirection flow ) => flow switch {
		FlowDirection.Down or FlowDirection.Up => LayoutDirection.Vertical,
		FlowDirection.Left or FlowDirection.Right => LayoutDirection.Horizontal,
		_ => LayoutDirection.Both
	};

	public static LayoutDirection GetFlowDirection ( this FlowDirection flow )
		=> flow.GetPrimaryDirection() is FlowDirection.Left or FlowDirection.Right
		? LayoutDirection.Horizontal
		: LayoutDirection.Vertical;

	public static LayoutDirection GetCrossDirection ( this FlowDirection flow ) => flow.GetSecondaryDirection() switch {
		FlowDirection.CrossLeft or FlowDirection.CrossRight => LayoutDirection.Horizontal,
		FlowDirection.CrossUp or FlowDirection.CrossDown => LayoutDirection.Vertical,
		_ => LayoutDirection.None
	};

	public static FlowSize2<T> ToFlow<T> ( this FlowDirection flow, Size2<T> size ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( size )
			: new( size.Height, size.Width );
	public static Size2<T> FromFlow<T> ( this FlowDirection flow, FlowSize2<T> size ) where T : INumber<T>
		=> flow.GetFlowDirection() == LayoutDirection.Horizontal
			? new( size )
			: new( size.Cross, size.Flow );

	public static FlowPoint2<T> ToFlow<T> ( this FlowDirection flow, Point2<T> point, FlowSize2<T> size ) where T : INumber<T> {
		var primary = flow.GetPrimaryDirection();
		var secondary = flow.GetSecondaryDirection();

		var aligned = primary is FlowDirection.Left or FlowDirection.Right
			? new FlowPoint2<T>( point )
			: new FlowPoint2<T>( point.Y, point.X );

		if ( primary is FlowDirection.Left or FlowDirection.Down ) aligned.Flow = size.Flow - aligned.Flow;
		if ( secondary is FlowDirection.CrossLeft or FlowDirection.CrossDown ) aligned.Cross = size.Cross - aligned.Cross;

		return aligned;
	}
	public static Point2<T> FromFlow<T> ( this FlowDirection flow, FlowPoint2<T> point, FlowSize2<T> size ) where T : INumber<T> {
		var primary = flow.GetPrimaryDirection();
		var secondary = flow.GetSecondaryDirection();

		if ( primary is FlowDirection.Left or FlowDirection.Down ) point.Flow = size.Flow - point.Flow;
		if ( secondary is FlowDirection.CrossLeft or FlowDirection.CrossDown ) point.Cross = size.Cross - point.Cross;

		var aligned = primary is FlowDirection.Left or FlowDirection.Right
			? new Point2<T>( point )
			: new Point2<T>( point.Cross, point.Flow );

		return aligned;
	}

	public static FlowSpacing<T> ToFlow<T> ( this FlowDirection flow, Spacing<T> spacing ) where T : INumber<T> {
		var primary = flow.GetPrimaryDirection();
		var secondary = flow.GetSecondaryDirection();

		var aligned = primary is FlowDirection.Left or FlowDirection.Right
			? new FlowSpacing<T> { FlowStart = spacing.Left, FlowEnd = spacing.Right, CrossStart = spacing.Bottom, CrossEnd = spacing.Top }
			: new FlowSpacing<T> { CrossStart = spacing.Left, CrossEnd = spacing.Right, FlowStart = spacing.Bottom, FlowEnd = spacing.Top };

		if ( primary is FlowDirection.Left or FlowDirection.Down ) (aligned.FlowStart, aligned.FlowEnd) = (aligned.FlowEnd, aligned.FlowStart);
		if ( secondary is FlowDirection.CrossLeft or FlowDirection.CrossDown ) (aligned.CrossStart, aligned.CrossEnd) = (aligned.CrossEnd, aligned.CrossStart);

		return aligned;
	}
}