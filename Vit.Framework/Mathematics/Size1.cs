/// This file [Size1.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.SizeTemplate and parameter 1 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Memory;

namespace Vit.Framework.Mathematics;

public struct Size1<T> : IInterpolatable<Size1<T>, T>, IEqualityOperators<Size1<T>, Size1<T>, bool>, IEquatable<Size1<T>>, IValueSpan<T> where T : INumber<T> {
	public T Width;
	
	public Size1 ( T width ) {
		Width = width;
	}
	
	#nullable disable
	public Size1 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public Size1 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly Size1<T> UnitWidth = new( T.One );
	public static readonly Size1<T> One = new( T.One );
	public static readonly Size1<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Width, 1 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Width, 1 );
	
	public Size1<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Width = Y.CreateChecked( Width )
		};
	}
	
	public Size1<T> Contain ( Size1<T> other ) => new() {
		Width = T.Max( Width, other.Width ),
	};
	
	public Size1<T> Lerp ( Size1<T> goal, T time ) {
		return new() {
			Width = Width.Lerp( goal.Width, time )
		};
	}
	
	public static Size1<T> operator * ( Size1<T> left, T right ) {
		return new() {
			Width = left.Width * right
		};
	}
	
	public static Size1<T> operator * ( T left, Size1<T> right ) {
		return new() {
			Width = left * right.Width
		};
	}
	
	public static Size1<T> operator / ( Size1<T> left, T right ) {
		return new() {
			Width = left.Width / right
		};
	}
	
	public static bool operator == ( Size1<T> left, Size1<T> right ) {
		return left.Width == right.Width;
	}
	
	public static bool operator != ( Size1<T> left, Size1<T> right ) {
		return left.Width != right.Width;
	}
	public static implicit operator Size1<T> ( T value )	=> new( value );
	public void Deconstruct ( out T width ) {
		width = Width;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is Size1<T> axes && Equals( axes );
	}
	
	public bool Equals ( Size1<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Width );
	}
	
	public override string ToString () {
		return $"{Width}";
	}
}
