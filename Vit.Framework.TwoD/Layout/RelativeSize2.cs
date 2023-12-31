﻿using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

public struct RelativeSize2<T> : IEqualityOperators<RelativeSize2<T>, RelativeSize2<T>, bool>, IInterpolatable<RelativeSize2<T>, T> where T : INumber<T> {
	public LayoutUnit<T> Width;
	public LayoutUnit<T> Height;

	public RelativeSize2 ( LayoutUnit<T> width, LayoutUnit<T> height ) {
		Width = width;
		Height = height;
	}

	public RelativeSize2 ( LayoutUnit<T> both ) {
		Width = Height = both;
	}

	public Size2<T> GetSize ( Size2<T> availableSpace ) => new() {
		Width = Width.GetValue( availableSpace.Width ),
		Height = Height.GetValue( availableSpace.Height )
	};

	public RelativeFlowSize2<T> ToFlow ( FlowDirection direction ) => direction.GetFlowDirection() == LayoutDirection.Horizontal
		? new( Width, Height )
		: new( Height, Width );

	public static implicit operator RelativeSize2<T> ( ValueTuple<T, T> tuple ) {
		return new( tuple.Item1, tuple.Item2 );
	}
	public void Deconstruct ( out LayoutUnit<T> width, out LayoutUnit<T> height ) {
		width = Width;
		height = Height;
	}

	public static implicit operator RelativeSize2<T> ( Size2<T> size ) => new() {
		Width = size.Width,
		Height = size.Height
	};

	public override string ToString () {
		return $"{Width}x{Height}";
	}

	public RelativeSize2<T> Lerp ( RelativeSize2<T> goal, T time ) {
		return new() {
			Width = Width.Lerp( goal.Width, time ),
			Height = Height.Lerp( goal.Height, time )
		};
	}

	public static bool operator == ( RelativeSize2<T> left, RelativeSize2<T> right ) {
		return left.Width == right.Width
			&& left.Height == right.Height;
	}

	public static bool operator != ( RelativeSize2<T> left, RelativeSize2<T> right ) {
		return left.Width != right.Width
			|| left.Height != right.Height;
	}
}
