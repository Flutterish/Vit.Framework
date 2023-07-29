/// This file [FlowVector2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Layout.FlowVectorTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.TwoD.Layout;

public struct FlowVector2<T> : IInterpolatable<FlowVector2<T>, T>, IEqualityOperators<FlowVector2<T>, FlowVector2<T>, bool>, IEquatable<FlowVector2<T>>, IValueSpan<T> where T : INumber<T> {
	public T Flow;
	public T Cross;

	public FlowVector2 ( T flow, T cross ) {
		Flow = flow;
		Cross = cross;
	}

	public FlowVector2 ( T all ) {
		Flow = Cross = all;
	}

#nullable disable
	public FlowVector2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( AsSpan() );
	}

	public FlowVector2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( AsSpan() );
	}
#nullable restore

	public static readonly FlowVector2<T> UnitFlow = new( T.One, T.Zero );
	public static readonly FlowVector2<T> UnitCross = new( T.Zero, T.One );
	public static readonly FlowVector2<T> One = new( T.One );
	public static readonly FlowVector2<T> Zero = new( T.Zero );


	public T LengthSquared => Flow * Flow + Cross * Cross;

	public FlowVector2<T> RotatedByFlowCross => new( -Cross, Flow );
	public FlowVector2<T> RotatedByCrossFlow => new( Cross, -Flow );

	public FlowVector2<T> Left => new( -Cross, Flow );
	public FlowVector2<T> Right => new( Cross, -Flow );

	public Mathematics.LinearAlgebra.Generic.Vector<T> AsUnsized () => new( AsSpan() );

	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Flow, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Flow, 2 );

	public FlowVector2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Flow = Y.CreateChecked( Flow ),
			Cross = Y.CreateChecked( Cross )
		};
	}

	public T Dot ( FlowVector2<T> other )
		=> Inner( this, other );
	public static T Dot ( FlowVector2<T> left, FlowVector2<T> right )
		=> Inner( left, right );
	public T Inner ( FlowVector2<T> other )
		=> Inner( this, other );
	public static T Inner ( FlowVector2<T> left, FlowVector2<T> right ) {
		return left.Flow * right.Flow
			+ left.Cross * right.Cross;
	}

	public FlowBiVector2<T> CrossProduct ( FlowVector2<T> other )
		=> Outer( this, other );
	public static FlowBiVector2<T> CrossProduct ( FlowVector2<T> left, FlowVector2<T> right )
		=> Outer( left, right );
	public FlowBiVector2<T> Outer ( FlowVector2<T> other )
		=> Outer( this, other );
	public static FlowBiVector2<T> Outer ( FlowVector2<T> left, FlowVector2<T> right ) {
		return new() {
			FlowCross = left.Flow * right.Cross - left.Cross * right.Flow,
		};
	}

	public FlowPoint2<T> FromOrigin () {
		return new() {
			Flow = Flow,
			Cross = Cross
		};
	}

	public FlowVector2<T> Lerp ( FlowVector2<T> goal, T time ) {
		return new() {
			Flow = Flow.Lerp( goal.Flow, time ),
			Cross = Cross.Lerp( goal.Cross, time )
		};
	}

	public static FlowVector2<T> operator + ( FlowVector2<T> left, FlowVector2<T> right ) {
		return new() {
			Flow = left.Flow + right.Flow,
			Cross = left.Cross + right.Cross
		};
	}

	public static FlowVector2<T> operator - ( FlowVector2<T> left, FlowVector2<T> right ) {
		return new() {
			Flow = left.Flow - right.Flow,
			Cross = left.Cross - right.Cross
		};
	}

	public static FlowVector2<T> operator - ( FlowVector2<T> vector ) {
		return new( -vector.Flow, -vector.Cross );
	}

	public static FlowVector2<T> operator * ( FlowVector2<T> vector, T scale ) {
		return new() {
			Flow = vector.Flow * scale,
			Cross = vector.Cross * scale
		};
	}

	public static FlowVector2<T> operator * ( T scale, FlowVector2<T> vector ) {
		return new() {
			Flow = scale * vector.Flow,
			Cross = scale * vector.Cross
		};
	}

	public static FlowVector2<T> operator / ( FlowVector2<T> vector, T divisor ) {
		return new() {
			Flow = vector.Flow / divisor,
			Cross = vector.Cross / divisor
		};
	}

	public static bool operator == ( FlowVector2<T> left, FlowVector2<T> right ) {
		return left.Flow == right.Flow
			&& left.Cross == right.Cross;
	}

	public static bool operator != ( FlowVector2<T> left, FlowVector2<T> right ) {
		return left.Flow != right.Flow
			|| left.Cross != right.Cross;
	}
	public static implicit operator FlowVector2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );

	public void Deconstruct ( out T flow, out T cross ) {
		flow = Flow;
		cross = Cross;
	}

	public override bool Equals ( object? obj ) {
		return obj is FlowVector2<T> axes && Equals( axes );
	}

	public bool Equals ( FlowVector2<T> other ) {
		return this == other;
	}

	public override int GetHashCode () {
		return HashCode.Combine( Flow, Cross );
	}

	public override string ToString () {
		return $"[{Flow}, {Cross}]";
	}
}

public static class Vector2Extensions {
	public static T GetLength<T> ( this FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Sqrt( vector.LengthSquared );
	}

	public static FlowVector2<T> Normalized<T> ( this FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector / vector.GetLength();
	}

	public static void Normalize<T> ( this ref FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.MultiplicativeIdentity / vector.GetLength();
		vector.Flow *= scale;
		vector.Cross *= scale;
	}

	public static T GetLengthFast<T> ( this FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	public static FlowVector2<T> NormalizedFast<T> ( this FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );
	}

	public static void NormalizeFast<T> ( this ref FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );
		vector.Flow *= scale;
		vector.Cross *= scale;
	}

	public static Radians<T> GetAngle<T> ( this FlowVector2<T> vector ) where T : IFloatingPointIeee754<T> {
		return T.Atan2( vector.Cross, vector.Flow ).Radians();
	}
}
