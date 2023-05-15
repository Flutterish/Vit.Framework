using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct LayoutUnit<T> where T : INumber<T> {
	public T Value;
	public bool IsRelative;

	public T GetValue ( T availableSpace )
		=> IsRelative ? Value * availableSpace : Value;

	public static implicit operator LayoutUnit<T> ( T value )
		=> new() { Value = value };

	public override string ToString () {
		return IsRelative ? $"{Value:P0}" : $"{Value}";
	}
}

public static class LayoutUnitExtensions {
	public static LayoutUnit<T> Relative<T> ( this T value ) where T : INumber<T> {
		return new LayoutUnit<T>() { Value = value, IsRelative = true };
	}
}