using System.Numerics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Mathematics;

/// <summary>
/// 2D matrix generator.
/// </summary>
public static class Transform2D<T> /*: DisposableObject*/ where T : INumber<T>, ITrigonometricFunctions<T> {
	public static Matrix3<T> Compute<TAngle> ( Point2<T> origin, Axes2<T> scale, Axes2<T> shear, TAngle rotation, Point2<T> position ) where TAngle : IAngle<TAngle, T> {
		return Matrix3<T>.CreateTranslation( origin.ToOrigin() ) *
			Matrix3<T>.CreateScale( scale ) *
			Matrix3<T>.CreateShear( shear ) *
			Matrix3<T>.CreateRotation( rotation ) *
			Matrix3<T>.CreateTranslation( position.FromOrigin() );
	}

	public static Matrix3<T> ComputeInverse<TAngle> ( Point2<T> origin, Axes2<T> scale, Axes2<T> shear, TAngle rotation, Point2<T> position ) where TAngle : IAngle<TAngle, T> {
		return Matrix3<T>.CreateTranslation( position.ToOrigin() ) *
			Matrix3<T>.CreateRotation( -rotation ) *
			Matrix3<T>.CreateShear( -shear ) *
			Matrix3<T>.CreateScale( T.One / scale.X, T.One / scale.Y ) *
			Matrix3<T>.CreateTranslation( origin.FromOrigin() );
	}

	//Transform2D<T>? parentTransform;
	//public Transform2D<T>? ParentTransform {
	//	get => parentTransform;
	//	set {
	//		if ( parentTransform == value )
	//			return;

	//		if ( parentTransform != null ) {
	//			parentTransform.MatrixInvalidated -= onParentMatrixInvalidated;
	//		}
	//		if ( value != null ) {
	//			value.MatrixInvalidated += onParentMatrixInvalidated;
	//		}

	//		parentTransform = value;
	//		onParentMatrixInvalidated();
	//	}
	//}

	//void onParentMatrixInvalidated () {
	//	unitToGlobal = null;
	//	unitToGlobalInverse = null;
	//	MatrixInvalidated?.Invoke();
	//}

	//protected override void Dispose ( bool disposing ) {
	//	parentTransform = null;
	//	MatrixInvalidated = null;
	//}

	//[MethodImpl( MethodImplOptions.AggressiveInlining )]
	//void trySet<K> ( ref K field, K value ) where K : IEqualityOperators<K, K, bool> {
	//	if ( field == value )
	//		return;

	//	field = value;

	//	if ( unitToLocal == null && unitToLocalInverse == null )
	//		return;

	//	unitToLocal = null;
	//	unitToLocalInverse = null;
	//	unitToGlobal = null;
	//	unitToGlobalInverse = null;
	//	MatrixInvalidated?.Invoke();
	//}

	//Point2<T> position;
	//public Point2<T> Position {
	//	get => position;
	//	set => trySet( ref position, value );
	//}
	//public T X {
	//	get => position.X;
	//	set => trySet( ref position.X, value );
	//}
	//public T Y {
	//	get => position.Y;
	//	set => trySet( ref position.Y, value );
	//}

	//Radians<T> rotation;
	//public Radians<T> Rotation {
	//	get => rotation;
	//	set => trySet( ref rotation, value );
	//}

	//Point2<T> origin;
	//public Point2<T> Origin {
	//	get => origin;
	//	set => trySet( ref origin, value );
	//}
	//public T OriginX {
	//	get => origin.X;
	//	set => trySet( ref origin.X, value );
	//}
	//public T OriginY {
	//	get => origin.Y;
	//	set => trySet( ref origin.Y, value );
	//}

	//Axes2<T> scale = Axes2<T>.One;
	//public Axes2<T> Scale {
	//	get => scale;
	//	set => trySet( ref scale, value );
	//}
	//public T ScaleX {
	//	get => scale.X;
	//	set => trySet( ref scale.X, value );
	//}
	//public T ScaleY {
	//	get => scale.Y;
	//	set => trySet( ref scale.Y, value );
	//}

	//Axes2<T> shear;
	//public Axes2<T> Shear {
	//	get => shear;
	//	set => trySet( ref shear, value );
	//}
	//public T ShearX {
	//	get => shear.X;
	//	set => trySet( ref shear.X, value );
	//}
	//public T ShearY {
	//	get => shear.Y;
	//	set => trySet( ref shear.Y, value );
	//}

	//Matrix3<T>? unitToLocal;
	//Matrix3<T>? unitToLocalInverse;
	///// <summary>
	///// A matrix such that (0,0) is mapped to the bottom left corner
	///// and (1,1) is mapped to the top right corner in parent space.
	///// </summary>
	//public Matrix3<T> UnitToLocalMatrix => unitToLocal ??= Compute( origin, scale, shear, rotation, position );
	///// <summary>
	///// A matrix such that the bottom left corner in parent space is mapped to (0,0)
	///// and the top right corner in parent space is mapped to (1,1).
	///// </summary>
	//public Matrix3<T> LocalToUnitMatrix => unitToLocalInverse ??= ComputeInverse( origin, scale, shear, rotation, position );

	//Matrix3<T>? unitToGlobal;
	//Matrix3<T>? unitToGlobalInverse;
	///// <summary>
	///// A matrix such that (0,0) is mapped to the bottom left corner
	///// and (1,1) is mapped to the top right corner in global space.
	///// </summary>
	//public Matrix3<T> UnitToGlobalMatrix => unitToGlobal ??= ParentTransform is null
	//	? UnitToLocalMatrix
	//	: (UnitToLocalMatrix * ParentTransform.UnitToGlobalMatrix);
	///// <summary>
	///// A matrix such that the bottom left corner in global space is mapped to (0,0)
	///// and the top right corner in global space is mapped to (1,1).
	///// </summary>
	//public Matrix3<T> GlobalToUnitMatrix => unitToGlobalInverse ??= ParentTransform is null
	//	? LocalToUnitMatrix
	//	: (ParentTransform.GlobalToUnitMatrix * LocalToUnitMatrix);

	//public event Action? MatrixInvalidated;
}
