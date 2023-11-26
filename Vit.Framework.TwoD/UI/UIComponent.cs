using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Timing;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI;

public abstract class UIComponent : IUIComponent, IHasAnimationTimeline {
	public UIComponent () {
		IHasEventTrees<UIComponent>.AddDeclaredEventHandlers( this, static ( d, t, h ) => d.AddEventHandler( t, h ) );
	}

	#region Hierarchy
	public int Depth { get; internal set; }
	ICompositeUIComponent<UIComponent>? parent;
	public ICompositeUIComponent<UIComponent>? Parent {
		get => parent;
		internal set {
			parent = value;
			OnParentMatrixInvalidated();
		}
	}
	#endregion
	#region Layout
	public LayoutInvalidations LayoutInvalidations { get; private set; } = LayoutInvalidations.Self;
	public void InvalidateLayout ( LayoutInvalidations invalidations ) {
		if ( IsComputingLayout && invalidations.HasFlag( LayoutInvalidations.Self ) ) {
			throw new InvalidOperationException( "Layout was invalidated while being computed. Recusive layout computations should be done internally and not over multiple layout calls" );
		}

		var combined = LayoutInvalidations | invalidations;
		if ( combined == LayoutInvalidations )
			return;

		LayoutInvalidations = combined;
		OnLayoutInvalidated( combined );
		Parent?.OnChildLayoutInvalidated( this, invalidations );
	}

	Size2<float>? requiredSize;
	/// <summary>
	/// The minimum size this element needs to display correctly, by an arbitrary metric depending on the type of component.
	/// </summary>
	/// <remarks>
	/// This usually indicates the space absolutely sized children in <see cref="IDrawableLayoutContainer"/>s occupy.
	/// </remarks>
	public Size2<float> RequiredSize => requiredSize ??= ComputeRequiredSize(); // TODO also we want this but with respect to min-width and min-height
	protected virtual Size2<float> ComputeRequiredSize () => Size2<float>.Zero;
	protected virtual void OnLayoutInvalidated ( LayoutInvalidations invalidations ) {
		if ( invalidations.HasFlag( LayoutInvalidations.Self | LayoutInvalidations.RequiredSize ) )
			requiredSize = null;
	}

	public bool IsComputingLayout { get; private set; }
	public void ComputeLayout () {
		if ( LayoutInvalidations == LayoutInvalidations.None )
			return;

		IsComputingLayout = true;
		PerformLayout();
		LayoutInvalidations = LayoutInvalidations.None;
		IsComputingLayout = false;
	}

	protected abstract void PerformLayout ();
	#endregion
	#region Transforms
	public virtual bool ReceivesPositionalInputAt ( Point2<float> screenSpacePosition ) {
		var localSpace = ScreenSpaceToLocalSpace( screenSpacePosition );

		return localSpace.X >= 0
			&& localSpace.Y >= 0
			&& localSpace.X <= Width
			&& localSpace.Y <= Height;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	void trySet<T> ( ref T field, T value ) where T : IEqualityOperators<T, T, bool> {
		if ( field == value )
			return;

		field = value;
		OnLocalMatrixInvalidated();
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

	Size2<float> size;
	/// <summary>
	/// The size allocated for this components layout, in parent space.
	/// </summary>
	public Size2<float> Size {
		get => size;
		set {
			if ( size == value )
				return;

			size = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}
	public float Width {
		get => size.Width;
		set {
			if ( size.Width == value )
				return;

			size.Width = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
	}
	public float Height {
		get => size.Height;
		set {
			if ( size.Height == value )
				return;

			size.Width = value;
			InvalidateLayout( LayoutInvalidations.Self );
		}
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

	protected void OnLocalMatrixInvalidated () {
		unitToLocal = null;
		unitToLocalInverse = null;

		if ( unitToGlobal == null && unitToGlobalInverse == null )
			return;

		unitToGlobal = null;
		unitToGlobalInverse = null;
		OnMatrixInvalidated();
	}
	protected virtual void OnMatrixInvalidated () { }
	internal void OnParentMatrixInvalidated () {
		if ( unitToGlobal == null && unitToGlobalInverse == null )
			return;

		unitToGlobal = null;
		unitToGlobalInverse = null;
		OnMatrixInvalidated();
	}

	public Point2<float> ScreenSpaceToLocalSpace ( Point2<float> point )
		=> GlobalToUnitMatrix.Apply( point );
	public Vector2<float> ScreenSpaceDeltaToLocalSpace ( Vector2<float> delta )
		=> GlobalToUnitMatrix.Apply( delta ) - GlobalToUnitMatrix.Apply( Vector2<float>.Zero );
	public Point2<float> LocalSpaceToScreenSpace ( Point2<float> point )
		=> UnitToGlobalMatrix.Apply( point );
	public Vector2<float> LocalSpaceDeltaToScreenSpace ( Vector2<float> delta )
		=> UnitToGlobalMatrix.Apply( delta ) - UnitToGlobalMatrix.Apply( Vector2<float>.Zero );
	public Point2<float> LocalSpaceToAnotherSpace ( Point2<float> point, IUIComponent other )
		=> other.ScreenSpaceToLocalSpace( LocalSpaceToScreenSpace( point ) );
	public Vector2<float> LocalSpaceDeltaToAnotherSpace ( Vector2<float> delta, IUIComponent other )
		=> other.ScreenSpaceDeltaToLocalSpace( LocalSpaceDeltaToScreenSpace( delta ) );

	Matrix3<float>? unitToLocal;
	Matrix3<float>? unitToLocalInverse;
	public Matrix3<float> UnitToLocalMatrix => unitToLocal ??= ComputeUnitToLocalMatrix();
	public Matrix3<float> LocalToUnitMatrix => unitToLocalInverse ??= ComputeLocalToUnitMatrix();

	protected virtual Matrix3<float> ComputeUnitToLocalMatrix () {
		return Matrix3<float>.CreateScale( scale ) *
			Matrix3<float>.CreateShear( shear ) *
			Matrix3<float>.CreateRotation( rotation ) *
			Matrix3<float>.CreateTranslation( position.FromOrigin() );
	}
	protected virtual Matrix3<float> ComputeLocalToUnitMatrix () {
		return Matrix3<float>.CreateTranslation( position.ToOrigin() ) *
			Matrix3<float>.CreateRotation( -rotation ) *
			Matrix3<float>.CreateShear( -shear ) *
			Matrix3<float>.CreateScale( 1 / scale.X, 1 / scale.Y );
	}

	Matrix3<float>? unitToGlobal;
	Matrix3<float>? unitToGlobalInverse;
	public Matrix3<float> UnitToGlobalMatrix => unitToGlobal ??= Parent is null
		? UnitToLocalMatrix
		: UnitToLocalMatrix * Parent.UnitToGlobalMatrix;
	public Matrix3<float> GlobalToUnitMatrix => unitToGlobalInverse ??= Parent is null
		? LocalToUnitMatrix
		: Parent.GlobalToUnitMatrix * LocalToUnitMatrix;
	#endregion
	#region Lifetime
	public IClock Clock { get; private set; } = null!; // NOTE maybe the timeline should be lazily initialized? there probably are way more components that are not animated
	public AnimationTimeline AnimationTimeline { get; } = new() { CurrentTime = double.NegativeInfinity.Millis() }; // negative infinity start time basically makes load-time animations finish instantly
	public bool IsLoaded { get; private set; }
	protected virtual void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		IsLoaded = true;
		Clock = dependencies.Resolve<IClock>();
	}
	public void Load ( IReadOnlyDependencyCache dependencies ) {
		if ( IsLoaded )
			return;

		OnLoad( dependencies );
		IsLoaded = true;
	}

	public void Unload () {
		if ( !IsLoaded )
			return;

		IsLoaded = false;
		OnUnload();
	}
	protected virtual void OnUnload () { }

	public virtual void Update () {
		AnimationTimeline.Update( Clock.CurrentTime );
	}

	public abstract DrawNode GetDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation;
	public abstract void DisposeDrawNodes ();

	public void Dispose ( RenderThreadScheduler disposeScheduler ) {
		disposeScheduler.ScheduleDrawNodeDisposal( this );
		Unload();
	}
	#endregion
	#region Events
	static Dictionary<Type, EventTree<UIComponent>> nullEventTree = new();
	Dictionary<Type, EventTree<UIComponent>>? handledEvents;
	public IReadOnlyDictionary<Type, EventTree<UIComponent>> HandledEventTypes => handledEvents ?? nullEventTree;
	protected EventTree<UIComponent> GetEventTree ( Type type ) {
		handledEvents ??= new();
		if ( handledEvents.TryGetValue( type, out var tree ) )
			return tree;

		tree = handledEvents[type] = new EventTree<UIComponent> {
			Source = this
		};
		EventHandlerAdded?.Invoke( type, tree );
		return tree;
	}
	protected bool TryGetEventTree ( Type type, [NotNullWhen( true )] out EventTree<UIComponent>? tree ) {
		if ( handledEvents == null ) {
			tree = null;
			return false;
		}

		return handledEvents.TryGetValue( type, out tree );
	}
	protected void TryTrimEventTree ( Type type ) {
		if ( handledEvents == null || !handledEvents.TryGetValue( type, out var tree ) )
			return;

		TryTrimEventTree( type, tree );
	}
	protected void TryTrimEventTree ( Type type, EventTree<UIComponent> tree ) {
		if ( tree.Handler != null || tree.Children.Count != 0 )
			return;

		handledEvents!.Remove( type );
		EventHandlerRemoved?.Invoke( type, tree );
	}

	protected void AddEventHandler<T> ( Func<T, bool> handler ) where T : Event {
		AddEventHandler( typeof( T ), e => handler( (T)e ) );
	}
	protected void AddEventHandler ( Type type, Func<Event, bool> handler ) {
		var tree = GetEventTree( type );
		if ( tree.Handler != null )
			throw new InvalidOperationException( "Can not have multiple handlers for the same event type. Did you mean to override the handler?" );

		tree.Handler = handler;
	}

	protected void RemoveEventHandler<T> () where T : Event {
		RemoveEventHandler( typeof( T ) );
	}
	protected void RemoveEventHandler ( Type type ) {
		if ( !TryGetEventTree( type, out var tree ) )
			return;

		tree.Handler = null;
		TryTrimEventTree( type, tree );
	}

	protected void OverrideEventHandler<T> ( Func<Func<Event, bool>, T, bool> handler ) where T : Event {
		OverrideEventHandler( typeof( T ), ( @base, e ) => handler( @base, (T)e ) );
	}
	protected void OverrideEventHandler ( Type type, Func<Func<Event, bool>, Event, bool> handler ) {
		if ( !TryGetEventTree( type, out var tree ) || tree.Handler == null )
			throw new InvalidOperationException( "No handler to override found" );

		var @base = tree.Handler;
		tree.Handler = e => handler( @base, e );
	}

	public event Action<Type, EventTree<UIComponent>>? EventHandlerAdded;
	public event Action<Type, EventTree<UIComponent>>? EventHandlerRemoved;
	#endregion

	public virtual DrawVisualizerBlueprint? CreateBlueprint () => null;
	Matrix3<float> IViewableInDrawVisualiser.UnitToGlobalMatrix => Matrix3<float>.CreateScale( Width, Height ) * UnitToGlobalMatrix;
}

public interface IUIComponent : IComponent<UIComponent>, IHasEventTrees<UIComponent>, IHasDrawNodes<DrawNode>, ICanReceivePositionalInput, IViewableInDrawVisualiser {
	new ICompositeUIComponent<UIComponent>? Parent { get; }
	IReadOnlyCompositeComponent<UIComponent, UIComponent>? IComponent<UIComponent>.Parent => Parent;
	/// <summary>
	/// Represents the index of this component in its parent (if it has a parent).
	/// </summary>
	int Depth { get; }

	void InvalidateLayout ( LayoutInvalidations invalidations );

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner,
	/// and (width,height) is mapped to the top right corner in parent space.
	/// </summary>
	Matrix3<float> UnitToLocalMatrix { get; }
	/// <summary>
	/// A matrix such that the bottom left corner in parent space is mapped to (0,0)
	/// and the top right corner in parent space is mapped to (width,height).
	/// </summary>
	Matrix3<float> LocalToUnitMatrix { get; }

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner,
	/// and (width,height) is mapped to the top right corner in global space.
	/// </summary>
	new Matrix3<float> UnitToGlobalMatrix { get; }
	/// <summary>
	/// A matrix such that the bottom left corner in global space is mapped to (0,0)
	/// and the top right corner in global space is mapped to (width,height).
	/// </summary>
	Matrix3<float> GlobalToUnitMatrix { get; }

	/// <summary>
	/// <inheritdoc cref="IDisposable.Dispose"/>
	/// </summary>
	/// <remarks>After disposing a component, you must ensure that it nor its children are accessed from the update thread.</remarks>
	/// <param name="disposeScheduler">The scheduler that will perform batch disposal of draw nodes.</param>
	void Dispose ( RenderThreadScheduler disposeScheduler );

	public Point2<float> ScreenSpaceToLocalSpace ( Point2<float> point )
		=> GlobalToUnitMatrix.Apply( point );
	public Vector2<float> ScreenSpaceDeltaToLocalSpace ( Vector2<float> delta )
		=> GlobalToUnitMatrix.Apply( delta ) - GlobalToUnitMatrix.Apply( Vector2<float>.Zero );
	public Point2<float> LocalSpaceToScreenSpace ( Point2<float> point )
		=> UnitToGlobalMatrix.Apply( point );
	public Vector2<float> LocalSpaceDeltaToScreenSpace ( Vector2<float> delta )
		=> UnitToGlobalMatrix.Apply( delta ) - UnitToGlobalMatrix.Apply( Vector2<float>.Zero );
	public Point2<float> LocalSpaceToAnotherSpace ( Point2<float> point, IUIComponent other )
		=> other.ScreenSpaceToLocalSpace( LocalSpaceToScreenSpace( point ) );
	public Vector2<float> LocalSpaceDeltaToAnotherSpace ( Vector2<float> delta, IUIComponent other )
		=> other.ScreenSpaceDeltaToLocalSpace( LocalSpaceDeltaToScreenSpace( delta ) );

	Size2<float> Size { get; }
	public float Width => Size.Width;
	public float Height => Size.Height;

	IViewableInDrawVisualiser? IViewableInDrawVisualiser.Parent => Parent;
	IEnumerable<IViewableInDrawVisualiser> IViewableInDrawVisualiser.Children => Array.Empty<IViewableInDrawVisualiser>();
	string IViewableInDrawVisualiser.Name => $"{GetType().Name} ({Size})";
}
