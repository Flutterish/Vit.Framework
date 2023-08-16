/// This file [Millis.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnitTemplate and parameter Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnits (Vit.Framework.Mathematics.SourceGen.Mathematics.TimeUnits)
using System.Numerics;

namespace Vit.Framework.Mathematics;

public struct Millis : IInterpolatable<Millis, double> {
	public double Value;
	
	public Millis ( double value ) {
		Value = value;
	}
	
	public Millis Lerp ( Millis goal, double time ) {
		return new( Value.Lerp( goal.Value, time ) );
	}
	
	public static readonly TimeSpan UnitSpan = new TimeSpan( 10000L );
	
	public static implicit operator TimeSpan ( Millis millis )
		=> UnitSpan * millis.Value;
	
	public static implicit operator Millis ( TimeSpan time )
		=> new( (double)time.Ticks / 10000L );
	
	public static Millis operator + ( Millis left, Millis right )
		=> new( left.Value + right.Value );
	
	public static Millis operator - ( Millis left, Millis right )
		=> new( left.Value - right.Value );
	
	public static double operator / ( Millis left, Millis right )
		=> left.Value / right.Value;
	
	public static bool operator == ( Millis left, Millis right )
		=> left.Value == right.Value;
	
	public static bool operator != ( Millis left, Millis right )
		=> left.Value != right.Value;
	
	public static bool operator > ( Millis left, Millis right )
		=> left.Value > right.Value;
	
	public static bool operator < ( Millis left, Millis right )
		=> left.Value < right.Value;
	
	public static bool operator >= ( Millis left, Millis right )
		=> left.Value >= right.Value;
	
	public static bool operator <= ( Millis left, Millis right )
		=> left.Value <= right.Value;
	
	public override string ToString () {
		return $"{Value}ms";
	}
}

public struct PerMilli<T> where T : IMultiplyOperators<T, double, T> {
	public T Value;
	
	public PerMilli ( T value ) {
		Value = value;
	}
	
	public static T operator * ( PerMilli<T> per, Millis time )
		=> per.Value * time.Value;
	
	public static T operator * ( Millis time, PerMilli<T> per )
		=> per.Value * time.Value;
	
	public override string ToString () {
		return $"{Value} per ms";
	}
}

public static class MillisExtensions {
	public static Millis Millis ( this double value )
		=> new( value );
	
	public static Millis Millis ( this int value )
		=> new( value );
	
	public static PerMilli<T> PerMilli<T> ( this T value ) where T : IMultiplyOperators<T, double, T>
		=> new( value );
}
