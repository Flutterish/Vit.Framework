/// This file [FlowSize2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Layout.FlowSizeTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.TwoD.Layout;

public struct FlowSize2<T> : IInterpolatable<FlowSize2<T>, T>, IEqualityOperators<FlowSize2<T>, FlowSize2<T>, bool>, IEquatable<FlowSize2<T>>, IValueSpan<T> where T : INumber<T> {
	public T Flow;
	public T Cross;
	
	public FlowSize2 ( T flow, T cross ) {
		Flow = flow;
		Cross = cross;
	}
	
	public FlowSize2 ( T all ) {
		Flow = Cross = all;
	}
	
	#nullable disable
	public FlowSize2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public FlowSize2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly FlowSize2<T> UnitFlow = new( T.One, T.Zero );
	public static readonly FlowSize2<T> UnitCross = new( T.Zero, T.One );
	public static readonly FlowSize2<T> One = new( T.One );
	public static readonly FlowSize2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Flow, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Flow, 2 );
	
	public FlowSize2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Flow = Y.CreateChecked( Flow ),
			Cross = Y.CreateChecked( Cross )
		};
	}
	
	public FlowSize2<T> Contain ( FlowSize2<T> other ) => new() {
		Flow = T.Max( Flow, other.Flow ),
		Cross = T.Max( Cross, other.Cross ),
	};
	
	public FlowSize2<T> Lerp ( FlowSize2<T> goal, T time ) {
		return new() {
			Flow = Flow.Lerp( goal.Flow, time ),
			Cross = Cross.Lerp( goal.Cross, time )
		};
	}
	
	public static FlowSize2<T> operator * ( FlowSize2<T> left, T right ) {
		return new() {
			Flow = left.Flow * right,
			Cross = left.Cross * right
		};
	}
	
	public static FlowSize2<T> operator * ( T left, FlowSize2<T> right ) {
		return new() {
			Flow = left * right.Flow,
			Cross = left * right.Cross
		};
	}
	
	public static FlowSize2<T> operator / ( FlowSize2<T> left, T right ) {
		return new() {
			Flow = left.Flow / right,
			Cross = left.Cross / right
		};
	}
	
	public static bool operator == ( FlowSize2<T> left, FlowSize2<T> right ) {
		return left.Flow == right.Flow
			&& left.Cross == right.Cross;
	}
	
	public static bool operator != ( FlowSize2<T> left, FlowSize2<T> right ) {
		return left.Flow != right.Flow
			|| left.Cross != right.Cross;
	}
	public static implicit operator FlowSize2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T flow, out T cross ) {
		flow = Flow;
		cross = Cross;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is FlowSize2<T> axes && Equals( axes );
	}
	
	public bool Equals ( FlowSize2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Flow, Cross );
	}
	
	public override string ToString () {
		return $"{Flow}x{Cross}";
	}
}
