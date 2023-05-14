using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct Padding<T> where T : INumber<T> {
	public T Top;
	public T Bottom;
	public T Left;
	public T Right;

	public T Vertical {
		get => Top + Bottom;
		set => Top = Bottom = value;
	}
	public T Horizontal {
		get => Left + Right;
		set => Left = Right = value;
	}

	public Padding ( T all ) {
		Top = Bottom = Left = Right = all;
	}
}
