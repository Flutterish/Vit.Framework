using System.Numerics;

namespace Vit.Framework.TwoD.Layout;

public struct LayoutUnit<T> : IEquatable<LayoutUnit<T>> where T : INumber<T> {
	public T Relative;
	public T Absolute;

	public T GetValue ( T availableSpace )
		=> Relative * availableSpace + Absolute;

	public static implicit operator LayoutUnit<T> ( T value )
		=> new() { Absolute = value };

	public static LayoutUnit<T> operator + ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return new() {
			Relative = left.Relative + right.Relative,
			Absolute = left.Absolute + right.Absolute
		};
	}

	public static LayoutUnit<T> operator - ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return new() {
			Relative = left.Relative - right.Relative,
			Absolute = left.Absolute - right.Absolute
		};
	}

	public static bool operator == ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return left.Relative == right.Relative
			&& left.Absolute == right.Absolute;
	}

	public static bool operator != ( LayoutUnit<T> left, LayoutUnit<T> right ) {
		return left.Relative != right.Relative
			|| left.Absolute != right.Absolute;
	}

	public override string ToString () {
		if ( Relative != T.Zero ) {
			if ( Absolute != T.Zero ) {
				return $"{Relative:P0} + {Absolute}";
			}
			else {
				return $"{Relative:P0}";
			}
		}
		else {
			return $"{Absolute}";
		}
	}

	public override bool Equals ( object? obj ) {
		return obj is LayoutUnit<T> unit && Equals( unit );
	}

	public bool Equals ( LayoutUnit<T> other ) {
		return this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Relative, Absolute );
	}
}

public static class LayoutUnitExtensions {
	public static LayoutUnit<T> Relative<T> ( this T value ) where T : INumber<T> {
		return new LayoutUnit<T>() { Relative = value };
	}
}