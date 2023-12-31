﻿using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics;

public struct ColorHsv<TAngle, T> 
	where T : INumber<T>, IFloatingPoint<T>
	where TAngle : IAngle<TAngle, T>
{
	public TAngle H;
	public T S;
	public T V;

	static readonly TAngle _180 = TAngle.FullRotation / (T.One + T.One);
	static readonly TAngle _60 = _180 / (T.One + T.One + T.One);
	static readonly TAngle _120 = _60 + _60;
	static readonly TAngle _240 = _180 + _60;
	static readonly TAngle _300 = _240 + _60;

	public ColorHsv ( TAngle h, T s, T v ) {
		H = h;
		S = s;
		V = v;
	}

	public ColorRgb<T> ToRgb () {
		var c = V * S;
		var h = H.Mod( TAngle.FullRotation );
		var x = c * ( T.One - T.Abs((h / _60).Mod(T.One + T.One) - T.One) );
		var m = V - c;

		var (r, g, b) = 
			h == TAngle.Zero ? (c, T.Zero, x) :
			h < _60 ? (c, x, T.Zero) :
			h < _120 ? (x, c, T.Zero) :
			h < _180 ? (T.Zero, c, x) :
			h < _240 ? (T.Zero, x, c) :
			h < _300 ? (x, T.Zero, c) :
			(c, T.Zero, x);

		return new ColorRgb<T>() {
			R = r + m,
			G = g + m,
			B = b + m
		};
	}

	public ColorRgba<T> ToRgba ( T opacity )
		=> ToRgb().WithOpacity( opacity );
}
