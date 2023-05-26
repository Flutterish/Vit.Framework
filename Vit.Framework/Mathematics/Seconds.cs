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
	
	public static readonly TimeSpan UnitSpan = new TimeSpan( (long)10000000 );
	
	public static implicit operator TimeSpan ( Seconds seconds )
		=> UnitSpan * seconds.Value;
	
	public override string ToString () {
		return $"{Value} seconds";
	}
}

public struct PerSecond<T> where T : IMultiplyOperators<T, double, T> {
	public T Value;
	
	public PerSecond ( T value ) {
		Value = value;
	}
	
	public override string ToString () {
		return $"{Value} per second";
	}
}

public static class SecondsExtensions {
	public static Seconds Seconds ( this double value )
		=> new( value );
}
