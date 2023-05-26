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
	
	public static readonly TimeSpan UnitSpan = new TimeSpan( (long)10000 );
	
	public static implicit operator TimeSpan ( Millis millis )
		=> UnitSpan * millis.Value;
	
	public override string ToString () {
		return $"{Value} millis";
	}
}

public struct PerMilli<T> where T : IMultiplyOperators<T, double, T> {
	public T Value;
	
	public PerMilli ( T value ) {
		Value = value;
	}
	
	public override string ToString () {
		return $"{Value} per milli";
	}
}

public static class MillisExtensions {
	public static Millis Millis ( this double value )
		=> new( value );
}
