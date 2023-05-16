/// This file [FlowAxes2.cs] was auto-generated with Vit.Framework.Mathematics.SourceGen.Layout.FlowAxesTemplate and parameter 2 (System.Int32)
using System.Numerics;
using System.Runtime.InteropServices;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct FlowAxes2<T> : IInterpolatable<FlowAxes2<T>, T>, IEqualityOperators<FlowAxes2<T>, FlowAxes2<T>, bool>, IEquatable<FlowAxes2<T>>, IValueSpan<T> where T : INumber<T> {
	public T Flow;
	public T Cross;
	
	public FlowAxes2 ( T flow, T cross ) {
		Flow = flow;
		Cross = cross;
	}
	
	public FlowAxes2 ( T all ) {
		Flow = Cross = all;
	}
	
	#nullable disable
	public FlowAxes2 ( ReadOnlySpan<T> span ) {
		span.CopyTo( this.AsSpan() );
	}
	
	public FlowAxes2 ( IReadOnlyValueSpan<T> span ) {
		span.AsReadOnlySpan().CopyTo( this.AsSpan() );
	}
	#nullable restore
	
	public static readonly FlowAxes2<T> UnitFlow = new( T.One, T.Zero );
	public static readonly FlowAxes2<T> UnitCross = new( T.Zero, T.One );
	public static readonly FlowAxes2<T> One = new( T.One );
	public static readonly FlowAxes2<T> Zero = new( T.Zero );
	
	
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Flow, 2 );
	public ReadOnlySpan<T> AsReadOnlySpan () => MemoryMarshal.CreateReadOnlySpan( ref Flow, 2 );
	
	public FlowAxes2<Y> Cast<Y> () where Y : INumber<Y> {
		return new() {
			Flow = Y.CreateChecked( Flow ),
			Cross = Y.CreateChecked( Cross )
		};
	}
	
	public FlowAxes2<T> Lerp ( FlowAxes2<T> goal, T time ) {
		return new() {
			Flow = Flow.Lerp( goal.Flow, time ),
			Cross = Cross.Lerp( goal.Cross, time )
		};
	}
	
	public static bool operator == ( FlowAxes2<T> left, FlowAxes2<T> right ) {
		return left.Flow == right.Flow
			&& left.Cross == right.Cross;
	}
	
	public static bool operator != ( FlowAxes2<T> left, FlowAxes2<T> right ) {
		return left.Flow != right.Flow
			|| left.Cross != right.Cross;
	}
	public static implicit operator FlowAxes2<T> ( (T, T) value )
		=> new( value.Item1, value.Item2 );
	
	public void Deconstruct ( out T flow, out T cross ) {
		flow = Flow;
		cross = Cross;
	}
	
	public override bool Equals ( object? obj ) {
		return obj is FlowAxes2<T> axes && Equals( axes );
	}
	
	public bool Equals ( FlowAxes2<T> other ) {
		return this == other;
	}
	
	public override int GetHashCode () {
		return HashCode.Combine( Flow, Cross );
	}
	
	public override string ToString () {
		return $"<{Flow}, {Cross}>";
	}
}
