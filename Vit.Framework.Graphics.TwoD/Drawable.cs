using System.Numerics;
using System.Runtime.CompilerServices;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public abstract partial class Drawable : DisposableObject, IDrawable {
	public int Depth { get; private set; }
	public void SetDepth ( int depth ) {
		Depth = depth;
	}

	public ICompositeDrawable<IDrawable>? Parent { get; private set; }
	void IDrawable.SetParent ( ICompositeDrawable<IDrawable>? parent ) {
		var old = Parent;

		Parent = parent;
		onParentMatrixInvalidated();

		OnParentChanged( old, parent );
	}

	protected virtual void OnParentChanged ( ICompositeDrawable<IDrawable>? from, ICompositeDrawable<IDrawable>? to ) { }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void trySet<T> ( ref T field, T value ) where T : IEqualityOperators<T, T, bool> {
		if ( field == value )
			return;

		field = value;

		if ( unitToLocal == null )
			return;

		unitToLocal = null;
		unitToLocalInverse = null;
		unitToGlobal = null;
		unitToGlobalInverse = null;
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
	public virtual void Update () { // TODO should "needs an update" be a flag?
		
	}

	protected RenderThreadScheduler DrawThreadScheduler { get; private set; } = null!;
	public void TryLoad ( IReadOnlyDependencyCache dependencies ) {
		if ( !IsLoaded ) {
			Load( dependencies );
			IsLoaded = true;
		}
	}
	protected virtual void Load ( IReadOnlyDependencyCache dependencies ) { // TODO should we have an "unload" method?
		DrawThreadScheduler = dependencies.Resolve<RenderThreadScheduler>();
	}

	public virtual bool ReceivesPositionalInputAt ( Point2<float> point ) {
		point = ScreenSpaceToLocalSpace( point );

		return point.X >= 0 && point.X <= 1
			&& point.Y >= 0 && point.Y <= 1;
	}

	void IDrawable.OnParentMatrixInvalidated ()
		=> onParentMatrixInvalidated();
	void onParentMatrixInvalidated () {
		if ( unitToGlobal == null )
			return;

		unitToGlobal = null;
		unitToGlobalInverse = null;
		OnMatrixInvalidated();
	}
	protected virtual void OnMatrixInvalidated () {
		InvalidateDrawNodes();
	}

	public Point2<float> ScreenSpaceToLocalSpace ( Point2<float> point )
		=> GlobalToUnitMatrix.Apply( point );

	public Point2<float> LocalSpaceToScreenSpace ( Point2<float> point )
		=> UnitToGlobalMatrix.Apply( point );

	Matrix3<float>? unitToLocal;
	Matrix3<float>? unitToLocalInverse;
	public Matrix3<float> UnitToLocalMatrix => unitToLocal ??=
		Matrix3<float>.CreateTranslation( origin.ToOrigin() ) *
		Matrix3<float>.CreateScale( scale ) *
		Matrix3<float>.CreateShear( shear ) *
		Matrix3<float>.CreateRotation( rotation ) *
		Matrix3<float>.CreateTranslation( position.FromOrigin() );
	public Matrix3<float> LocalToUnitMatrix => unitToLocalInverse ??=
		Matrix3<float>.CreateTranslation( position.ToOrigin() ) *
		Matrix3<float>.CreateRotation( -rotation ) *
		Matrix3<float>.CreateShear( -shear ) *
		Matrix3<float>.CreateScale( 1 / scale.X, 1 / scale.Y ) *
		Matrix3<float>.CreateTranslation( origin.FromOrigin() );

	Matrix3<float>? unitToGlobal;
	Matrix3<float>? unitToGlobalInverse;
	public Matrix3<float> UnitToGlobalMatrix => unitToGlobal ??= Parent is null
		? UnitToLocalMatrix
		: (UnitToLocalMatrix * Parent.UnitToGlobalMatrix);
	public Matrix3<float> GlobalToUnitMatrix => unitToGlobalInverse ??= Parent is null
		? LocalToUnitMatrix
		: (Parent.GlobalToUnitMatrix * LocalToUnitMatrix);

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode ( int subtreeIndex );

	public virtual DrawNode GetDrawNode ( int subtreeIndex ) {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode( subtreeIndex );
		node.Update();
		return node;
	}

	protected override void Dispose ( bool disposing ) { // TODO properly dispose of drawables and dispose of the whole tree when clearing
		DrawThreadScheduler.ScheduleDisposal( this );
	}
}

public interface IDrawable : IComponent<IDrawable>, IHas2DTransform, IHasEventTrees<IDrawable>, IDisposable {
	/// <summary>
	/// Child depth, indicates the index of the child inside the parent.
	/// </summary>
	int Depth { get; }
	/// <summary>
	/// Sets <see cref="Depth"/>. This should only be done by the parent.
	/// </summary>
	void SetDepth ( int depth );

	new ICompositeDrawable<IDrawable>? Parent { get; }
	/// <summary>
	/// Sets the <see cref="Parent"/> property. This should be synchronised with the parents children list, usually in the parents Add method.
	/// </summary>
	void SetParent ( ICompositeDrawable<IDrawable>? parent );
	IReadOnlyCompositeComponent<IDrawable, IDrawable>? IComponent<IDrawable>.Parent => Parent;

	public bool IsLoaded { get; }

	void TryLoad ( IReadOnlyDependencyCache dependencies );

	/// <summary>
	/// Checks if a given point lies on this drawable.
	/// </summary>
	/// <param name="point">A point in global space.</param>
	bool ReceivesPositionalInputAt ( Point2<float> point );

	void OnParentMatrixInvalidated ();

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner
	/// and (1,1) is mapped to the top right corner in parent space.
	/// </summary>
	Matrix3<float> UnitToLocalMatrix { get; }

	/// <summary>
	/// A matrix such that the bottom left corner in parent space is mapped to (0,0)
	/// and the top right corner in parent space is mapped to (1,1).
	/// </summary>
	Matrix3<float> LocalToUnitMatrix { get; }

	void Update ();

	DrawNode GetDrawNode ( int subtreeIndex );
}