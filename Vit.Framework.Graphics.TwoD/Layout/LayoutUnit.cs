using System.Numerics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct LayoutUnit<T> : IEquatable<LayoutUnit<T>> where T : INumber<T> {
	public T Value;
	public bool IsRelative;

	public T GetValue ( T availableSpace )
		=> IsRelative ? Value * availableSpace : Value;

	public static implicit operator LayoutUnit<T> ( T value )
		=> new() { Value = value };

	public static bool operator == ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return left.IsRelative == right.IsRelative
			&& left.Value == right.Value;
	}

	public static bool operator != ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return left.IsRelative != right.IsRelative
			|| left.Value != right.Value;
	}

	public override string ToString () {
		return IsRelative ? $"{Value:P0}" : $"{Value}";
	}

	public override bool Equals ( object? obj ) {
		return obj is LayoutUnit<T> unit && Equals( unit );
	}

	public bool Equals ( LayoutUnit<T> other ) {
		return this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Value, IsRelative );
	}
}

public static class LayoutUnitExtensions {
	public static LayoutUnit<T> Relative<T> ( this T value ) where T : INumber<T> {
		return new LayoutUnit<T>() { Value = value, IsRelative = true };
	}
}