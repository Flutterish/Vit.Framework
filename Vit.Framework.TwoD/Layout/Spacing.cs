using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Layout;

public struct Spacing<T> : IInterpolatable<Spacing<T>, T> where T : INumber<T> {
	public T Bottom;
	public T Top;
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

	public Spacing ( T all ) {
		Top = Bottom = Left = Right = all;
	}

	public Spacing<T> Lerp ( Spacing<T> goal, T time ) {
		return new() {
			Bottom = Bottom.Lerp( goal.Bottom, time ),
			Top = Top.Lerp( goal.Top, time ),
			Left = Left.Lerp( goal.Left, time ),
			Right = Right.Lerp( goal.Right, time )
		};
	}

	public override string ToString () {
		return $"[Left = {Left}; Right = {Right}; Top = {Top}; Bottom = {Bottom}]";
	}
}
