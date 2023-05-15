/// This file [FlowPoint2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Layout.FlowPointTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct FlowPoint2<T> : IInterpolatable<FlowPoint2<T>, T>, IEqualityOperators<FlowPoint2<T>, FlowPoint2<T>, bool>, IEquatable<FlowPoint2<T>>, IValueSpan<T> where T : INumber<T> {
	public T Flow;
	public T Cross;
	
	public FlowPoint2 ( T flow, T cross ) {
		Flow = flow;
		Cross = cross;
	}
	
	public FlowPoint2 ( T all ) {
		Flow = Cross = all;
	}
	
	#nullable disable
	public FlowPoint2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public FlowPoint2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly FlowPoint2<T> UnitFlow = new( T.One, T.Zero );
	public static readonly FlowPoint2<T> UnitCross = new( T.Zero, T.One );
	public static readonly FlowPoint2<T> One = new( T.One );
	public static readonly FlowPoint2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Flow, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Flow, 2 );
	
	public FlowPoint2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Flow = Y.CreateChecked( Flow ),
			Cross = Y.CreateChecked( Cross )
		};
	}
	
	public FlowVector2<T> FromOrigin () {
		return new() {
			Flow = Flow,
			Cross = Cross
		};
	}
	
	public FlowVector2<T> ToOrigin () {
		return new() {
			Flow = -Flow,
			Cross = -Cross
		};
	}
	
	public FlowPoint2<T> ScaleAboutOrigin ( T scale ) {
		return new() {
			Flow = Flow * scale,
			Cross = Cross * scale
		};
	}
	
	public FlowPoint2<T> ReflectAboutOrigin () {
		return new() {
			Flow = -Flow,
			Cross = -Cross
		};
	}
	
	public FlowPoint2<T> Lerp ( FlowPoint2<T> goal, T time ) {
		return new() {
			Flow = Flow.Lerp( goal.Flow, time ),
			Cross = Cross.Lerp( goal.Cross, time )
		};
	}
	
	public static FlowPoint2<T> operator + ( FlowPoint2<T> point, FlowVector2<T> delta ) {
		return new() {
			Flow = point.Flow + delta.Flow,
			Cross = point.Cross + delta.Cross
		};
	}
	
	public static FlowPoint2<T> operator - ( FlowPoint2<T> point, FlowVector2<T> delta ) {
		return new() {
			Flow = point.Flow - delta.Flow,
			Cross = point.Cross - delta.Cross
		};
	}
	
	public static FlowVector2<T> operator - ( FlowPoint2<T> to, FlowPoint2<T> from ) {
		return new() {
			Flow = to.Flow - from.Flow,
			Cross = to.Cross - from.Cross
		};
	}
	
	public static bool operator == ( FlowPoint2<T> left, FlowPoint2<T> right ) {
		return left.Flow == right.Flow
			&& left.Cross == right.Cross;
	}
	
	public static bool operator != ( FlowPoint2<T> left, FlowPoint2<T> right ) {
		return left.Flow != right.Flow
			|| left.Cross != right.Cross;
	}
	public static implicit operator FlowPoint2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T flow, out T cross ) {
		flow = Flow;
		cross = Cross;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is FlowPoint2<T> axes && Equals( axes );
	}
	
	public bool Equals ( FlowPoint2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Flow, Cross );
	}
	
	public override string ToString () {
		return $"({Flow}, {Cross})";
	}
}
