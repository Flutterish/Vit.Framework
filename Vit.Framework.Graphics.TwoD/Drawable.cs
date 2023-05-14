using System.Numerics;
using System.Runtime.CompilerServices;
using Vit.Framework.Hierarchy;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public abstract partial class Drawable : DisposableObject, IDrawable {
	public ICompositeDrawable<Drawable>? Parent { get; private set; }
	void IDrawable.SetParent ( ICompositeDrawable<Drawable>? parent ) {
		if ( Parent != null && parent != null ) {
			throw new InvalidOperationException( $"Drawable may not have multiple parents." );
		}

		Parent = parent;
		OnParentMatrixInvalidated();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void trySet<T> ( ref T field, T value ) where T : IEqualityOperators<T, T, bool> {
		if ( field == value )
			return;

		field = value;

		if ( unitToLocal == null )
			return;

		unitToLocal = null;
		unitToGlobal = null;
		OnMatrixInvalidated();
	}

	Point2<float> position;
	public Point2<float> Position {
		get => position;
		set => trySet( ref position, value );
	}
	public float X {
		get => position.X;
		set => trySet( ref position.X, value );
	}
	public float Y {
		get => position.Y;
		set => trySet( ref position.Y, value );
	}

	Radians<float> rotation;
	public Radians<float> Rotation {
		get => rotation;
		set => trySet( ref rotation, value );
	}

	Point2<float> origin;
	public Point2<float> Origin {
		get => origin;
		set => trySet( ref origin, value );
	}
	public float OriginX {
		get => origin.X;
		set => trySet( ref origin.X, value );
	}
	public float OriginY {
		get => origin.Y;
		set => trySet( ref origin.Y, value );
	}

	Axes2<float> scale = Axes2<float>.One;
	public Axes2<float> Scale {
		get => scale;
		set => trySet( ref scale, value );
	}
	public float ScaleX {
		get => scale.X;
		set => trySet( ref scale.X, value );
	}
	public float ScaleY {
		get => scale.Y;
		set => trySet( ref scale.Y, value );
	}

	Axes2<float> shear;
	public Axes2<float> Shear {
		get => shear;
		set => trySet( ref shear, value );
	}
	public float ShearX {
		get => shear.X;
		set => trySet( ref shear.X, value );
	}
	public float ShearY {
		get => shear.Y;
		set => trySet( ref shear.Y, value );
	}

	public bool IsLoaded { get; private set; }
	public virtual void Update () {
		
	}

	public void TryLoad () {
		if ( !IsLoaded ) {
			Load();
			IsLoaded = true;
		}
	}
	protected virtual void Load () { // TODO should we have an "unload" method?

	}

	internal void OnParentMatrixInvalidated () {
		if ( unitToGlobal == null )
			return;

		unitToGlobal = null;
		OnMatrixInvalidated();
	}
	protected virtual void OnMatrixInvalidated () {
		InvalidateDrawNodes();
	}

	Matrix3<float>? unitToLocal;
	public Matrix3<float> UnitToLocalMatrix => unitToLocal ??=
		Matrix3<float>.CreateTranslation( origin.ToOrigin() ) *
		Matrix3<float>.CreateScale( scale ) *
		Matrix3<float>.CreateShear( shear ) *
		Matrix3<float>.CreateRotation( rotation ) *
		Matrix3<float>.CreateTranslation( position.FromOrigin() );

	Matrix3<float>? unitToGlobal;
	public Matrix3<float> UnitToGlobalMatrix => unitToGlobal ??= Parent is null
		? UnitToLocalMatrix
		: (UnitToLocalMatrix * Parent.UnitToGlobalMatrix);

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode ( int subtreeIndex );

	public virtual DrawNode GetDrawNode ( int subtreeIndex ) {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode( subtreeIndex );
		node.Update();
		return node;
	}

	protected override void Dispose ( bool disposing ) {
		
	}
}

public interface IDrawable : IComponent<Drawable>, IDisposable {
	new ICompositeDrawable<Drawable>? Parent { get; }
	internal void SetParent ( ICompositeDrawable<Drawable>? parent );
	IReadOnlyCompositeComponent<Drawable, Drawable>? IComponent<Drawable>.Parent => Parent;

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner
	/// and (1,1) is mapped to the top right corner in parent space.
	/// </summary>
	Matrix3<float> UnitToLocalMatrix { get; }

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner
	/// and (1,1) is mapped to the top right corner in global space.
	/// </summary>
	Matrix3<float> UnitToGlobalMatrix { get; }
}