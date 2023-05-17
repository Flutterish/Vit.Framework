using System.Numerics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.Layout;

public struct SizeBounds2<T> where T : INumber<T> {
	public RelativeSize2<T> Base;
	public RelativeSize2<T>? Min;
	public RelativeSize2<T>? Max;

	public FlowSizeBounds2<T> ToFlow ( FlowDirection direction ) => new() {
		Base = Base.ToFlow( direction ),
		Min = Min?.ToFlow( direction ),
		Max = Max?.ToFlow( direction )
	};

	public static implicit operator SizeBounds2<T> ( RelativeSize2<T> size ) => new() {
		Base = size
	};
	public static implicit operator SizeBounds2<T> ( Size2<T> size ) => new() {
		Base = size
	};
}
