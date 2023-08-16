/// This file [Seconds.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnitTemplate and parameter Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnits (Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnits)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Seconds : IInterpolatable<Seconds, double> {
	public double Value;
	
	public Seconds ( double value ) {
		Value = value;
	}
	
	public Seconds Lerp ( Seconds goal, double time ) {
		return new( Value.Lerp( goal.Value, time ) );
	}
	
	public static readonly TimeSpan UnitSpan = new TimeSpan( 10000000L );
	
	public static implicit operator TimeSpan ( Seconds seconds )
		=> UnitSpan * seconds.Value;
	
	public static implicit operator Seconds ( TimeSpan time )
		=> new( (double)time.Ticks / 10000000L );
	
	public static Seconds operator + ( Seconds left, Seconds right )
		=> new( left.Value + right.Value );
	
	public static Seconds operator - ( Seconds left, Seconds right )
		=> new( left.Value - right.Value );
	
	public static double operator / ( Seconds left, Seconds right )
		=> left.Value / right.Value;
	
	public static bool operator == ( Seconds left, Seconds right )
		=> left.Value == right.Value;
	
	public static bool operator != ( Seconds left, Seconds right )
		=> left.Value != right.Value;
	
	public static bool operator > ( Seconds left, Seconds right )
		=> left.Value > right.Value;
	
	public static bool operator < ( Seconds left, Seconds right )
		=> left.Value < right.Value;
	
	public static bool operator >= ( Seconds left, Seconds right )
		=> left.Value >= right.Value;
	
	public static bool operator <= ( Seconds left, Seconds right )
		=> left.Value <= right.Value;
	
	public override string ToString () {
		return $"{Value}s";
	}
}

public struct PerSecond<T> where T : IMultiplyOperators<T, double, T> {
	public T Value;
	
	public PerSecond ( T value ) {
		Value = value;
	}
	
	public static T operator * ( PerSecond<T> per, Seconds time )
		=> per.Value * time.Value;
	
	public static T operator * ( Seconds time, PerSecond<T> per )
		=> per.Value * time.Value;
	
	public override string ToString () {
		return $"{Value} per s";
	}
}

public static class SecondsExtensions {
	public static Seconds Seconds ( this double value )
		=> new( value );
	
	public static Seconds Seconds ( this int value )
		=> new( value );
	
	public static PerSecond<T> PerSecond<T> ( this T value ) where T : IMultiplyOperators<T, double, T>
		=> new( value );
}
