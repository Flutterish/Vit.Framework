﻿using System.Collections;
using System.Runtime.InteropServices;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Hierarchy;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;

namespace Vit.Framework.TwoD.UI.Composite;

public abstract class CompositeUIComponent<TChild, TChildData, TChildPolicy> : UIComponent, IReadOnlyList<TChild>, ICompositeUIComponent<TChild> 
	where TChild : UIComponent 
	where TChildData : struct, IChildData<TChild>
	where TChildPolicy : struct, IChildPolicy<TChild> 
{
	List<TChildData> internalChildren = new();
	public IReadOnlyList<TChild> Children {
		get => this;
	}

	protected IReadOnlyList<TChildData> InternalChildren {
		get => internalChildren;
		init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}

	protected TChildData InternalChild {
		get => internalChildren.Single();
		init => AddInternalChild( value );
	}

	protected ref TChildData ChildDataAt ( int index )
		=> ref CollectionsMarshal.AsSpan( internalChildren )[index];

	public IReadOnlyDependencyCache Dependencies { get; private set; } = null!;

	bool isMaskingActive;
	/// <summary>
	/// Masking hides parts of children which are outside of the parent.
	/// It also enables border effects such as rounded corners.
	/// </summary>
	/// <remarks>
	/// Masking containers <b>can</b> be nested - the resulting mask is a set intersection of both masks.
	/// More complex masks (such as mask unions or symmetric differences) can be created with custom draw nodes using the <see cref="MaskingDataBuffer"/>.
	/// </remarks>
	protected bool IsMaskingActive {
		get => isMaskingActive;
		set {
			if ( value.TrySet( ref isMaskingActive ) )
				invalidateDrawNodes();
		}
	}

	Corners<float> cornerExponents = 2.5f;
	/// <summary>
	/// Exponent of each corner respecitvely. The equation used is <c>|x|^exponent + |y|^exponent &lt;= 1</c> (adjusted for size and corner radius).
	/// <list type="table">
	///		<item>A value of 0 results in a completely cut-out corner.</item>
	///		<item>A value of 0-1 results in convave arcs.</item>
	///		<item>A value of 1 results in straight lines.</item>
	///		<item>A value of 1-2 results in bevels with sharp corners.</item>
	///		<item>A value of 2 results in circular arcs.</item>
	///		<item>A value of 2.5 results in something similar to apple's smooth corner (the default value).</item>
	///		<item>Higher values results in progressively more square squircles.</item>
	///		<item>We do not speak about negative values.</item>
	/// </list>
	/// </summary>
	/// <remarks>
	/// Requires <see cref="IsMaskingActive"/> to be <see langword="true"/> to have any effect.
	/// An interactive demo can be found <see href="https://www.desmos.com/calculator/bdnfusuk9o">here</see>.
	/// </remarks>
	protected Corners<float> CornerExponents {
		get => cornerExponents;
		set {
			if ( value.TrySet( ref cornerExponents ) )
				invalidateDrawNodes();
		}
	}

	Corners<Axes2<float>> cornerRadii;
	/// <summary>
	/// Radius of each corner respectively.
	/// </summary>
	/// <remarks>
	/// Requires <see cref="IsMaskingActive"/> to be <see langword="true"/> to have any effect.
	/// </remarks>
	protected Corners<Axes2<float>> CornerRadii {
		get => cornerRadii;
		set {
			if ( value.TrySet( ref cornerRadii ) )
				invalidateDrawNodes();
		}
	}

	public override void UpdateMasking ( AxisAlignedBox2<float> maskingBounds ) {
		base.UpdateMasking( maskingBounds );
		if ( !IsVisible )
			return;

		if ( IsMaskingActive ) 
			maskingBounds = maskingBounds.Intersect( ScreenSpaceQuad.BoundingBox );

		TChildPolicy.UpdateMasking( Children, maskingBounds );
	}

	protected void AddInternalChild ( TChildData data ) {
		var child = data.Child;
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = internalChildren.Count;
		internalChildren.Add( data );
		if ( TChildPolicy.LoadChildren && IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}
	protected void InsertInternalChild ( int index, TChildData data ) {
		var child = data.Child;
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = index;
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Child.Depth++;
		}
		internalChildren.Insert( index, data );
		if ( TChildPolicy.LoadChildren && IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}

	protected void RemoveInternalChild ( TChild child ) {
		var index = child.Depth;
		if ( internalChildren[index].Child != child )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		RemoveInternalChildAt( index );
	}
	protected void RemoveInternalChildAt ( int index ) {
		var child = internalChildren[index].Child;

		child.Parent = null;
		internalChildren.RemoveAt( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Child.Depth--;
		}
		if ( TChildPolicy.LoadChildren && child.IsLoaded )
			child.Unload();
		onChildRemoved( child );
	}

	protected void NoUnloadRemoveInternalChild ( TChild child ) {
		var index = child.Depth;
		if ( internalChildren[index].Child != child )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		NoUnloadRemoveInternalChildAt( index );
	}
	protected void NoUnloadRemoveInternalChildAt ( int index ) {
		var child = internalChildren[index].Child;

		child.Parent = null;
		internalChildren.RemoveAt( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Child.Depth--;
		}
		onChildRemoved( child );
	}

	protected void ClearInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			RemoveInternalChildAt( internalChildren.Count - 1 );
		}
	}

	protected void NoUnloadClearInternalChildren () {
		while ( internalChildren.Count != 0 ) {
			NoUnloadRemoveInternalChildAt( internalChildren.Count - 1 );
		}
	}

	protected void DisposeInternalChildren ( RenderThreadScheduler disposeScheduler ) {
		while ( internalChildren.Count != 0 ) {
			var child = internalChildren[^1].Child;
			RemoveInternalChildAt( internalChildren.Count - 1 );
			child.Dispose( disposeScheduler );
		}
	}

	void onChildAdded ( TChild child ) {
		if ( TChildPolicy.ProcessChildEvents ) {
			child.EventHandlerAdded += onChildEventHandlerAdded;
			child.EventHandlerRemoved += onChildEventHandlerRemoved;
			foreach ( var (type, tree) in child.HandledEventTypes ) {
				onChildEventHandlerAdded( type, tree );
			}
		}
		ChildAdded?.Invoke( this, child );
		InvalidateLayout( LayoutInvalidations.Children );
		invalidateDrawNodes();
	}

	void onChildRemoved ( TChild child ) {
		if ( TChildPolicy.ProcessChildEvents ) {
			child.EventHandlerAdded -= onChildEventHandlerAdded;
			child.EventHandlerRemoved -= onChildEventHandlerRemoved;
			foreach ( var (type, tree) in child.HandledEventTypes ) {
				onChildEventHandlerRemoved( type, tree );
			}
		}
		ChildRemoved?.Invoke( this, child );
		InvalidateLayout( LayoutInvalidations.Children );
		invalidateDrawNodes();
	}

	void onChildEventHandlerRemoved ( Type type, EventTree<UIComponent> tree ) {
		if ( type.IsAssignableTo( typeof( INonPropagableEvent ) ) )
			return;

		var ourTree = GetEventTree( type );
		ourTree.Remove( tree );
		TryTrimEventTree( type );

		sortEventTree( ourTree );
	}

	void onChildEventHandlerAdded ( Type type, EventTree<UIComponent> tree ) {
		if ( type.IsAssignableTo( typeof( INonPropagableEvent ) ) )
			return;

		var ourTree = GetEventTree( type );
		ourTree.Add( tree );

		sortEventTree( ourTree );
	}

	void sortEventTree ( EventTree<UIComponent> tree ) {
		TChildPolicy.SortEventTree( tree );
	}

	protected virtual IReadOnlyDependencyCache CreateDependencies ( IReadOnlyDependencyCache parent ) {
		return parent;
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		maskingData = dependencies.Resolve<MaskingDataBuffer>();
		Dependencies = CreateDependencies( dependencies );

		if ( TChildPolicy.LoadChildren ) {
			foreach ( var i in internalChildren ) {
				i.Child.Load( Dependencies );
			}
		}
	}
	protected override void OnUnload () {
		if ( TChildPolicy.LoadChildren ) {
			foreach ( var i in internalChildren.Reverse<TChildData>() ) {
				i.Child.Unload();
			}
		}
		Dependencies = null!;
	}

	public override void Update () {
		base.Update();
		TChildPolicy.UpdateSubtree( Children );
	}

	protected override void OnMatrixInvalidated () {
		base.OnMatrixInvalidated();
		if ( TChildPolicy.InvalidateChildMatrices ) {
			foreach ( var i in internalChildren ) {
				i.Child.OnParentMatrixInvalidated();
			}
		}
	}

	public virtual void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations ) {
		InvalidateLayout( LayoutInvalidations.Children );
	}

	protected sealed override void PerformLayout () {
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Self ) ) 
			PerformSelfLayout();
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Children ) ) {
			foreach ( var i in Children ) {
				i.ComputeLayout();
			}
		}
	}

	protected virtual void PerformSelfLayout () { }

	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<TChild>, TChild>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<TChild>, TChild>? ChildRemoved;

	void invalidateDrawNodes () {
		if ( drawNodeInvalidations.InvalidateDrawNodes() )
			Parent?.OnChildDrawNodesInvalidated();
	}
	public void OnChildDrawNodesInvalidated () {
		invalidateDrawNodes();
	}
	DrawNodeInvalidations drawNodeInvalidations;
	DrawNode?[] drawNodes = new DrawNode?[3];

	public override void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection ) {
		if ( IsMaskingActive ) {
			var node = drawNodes[subtreeIndex] ??= CreateDrawNode<TSpecialisation>( subtreeIndex );
			node.Update();
			collection.Add( node );
		}
		else {
			drawNodeInvalidations.ValidateDrawNode( subtreeIndex );
			TChildPolicy.PopulateDrawNodes<TSpecialisation>( subtreeIndex, collection, Children );
		}
	}

	protected virtual MaskingDrawNode<TSpecialisation> CreateDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		return new MaskingDrawNode<TSpecialisation>( this, subtreeIndex );
	}

	public override void DisposeDrawNodes () {
		invalidateDrawNodes();
		for ( int i = 0; i < 3; i++ ) {
			drawNodes[i]?.Dispose();
			drawNodes[i] = null;
		}

		foreach ( var i in Children ) {
			i.DisposeDrawNodes();
		}
	}

	MaskingDataBuffer maskingData = null!;
	public class MaskingDrawNode<TSpecialisation> : DrawNode where TSpecialisation : unmanaged, IRendererSpecialisation {
		protected readonly CompositeUIComponent<TChild, TChildData, TChildPolicy> Source;
		public MaskingDrawNode ( CompositeUIComponent<TChild, TChildData, TChildPolicy> source, int subtreeIndex ) : base( subtreeIndex ) {
			Source = source;
		}

		protected Corners<float> CornerExponents;
		protected Corners<Axes2<float>> CornerRadii;
		protected Matrix3<float> GlobalToUnit;
		DrawNodeCollection children = new();
		public sealed override void Update () {
			if ( !Source.drawNodeInvalidations.ValidateDrawNode( SubtreeIndex ) )
				return;

			UpdateState();
		}

		protected virtual void UpdateState () {
			GlobalToUnit = Source.GlobalToUnitMatrix * Matrix3<float>.CreateScale( 1f / Source.Width, 1f / Source.Height );
			CornerExponents = Source.cornerExponents;
			CornerRadii = MaskingData.NormalizeCornerRadii( Source.cornerRadii, Source.Size );
			children.Clear();
			TChildPolicy.PopulateDrawNodes<TSpecialisation>( SubtreeIndex, children, Source.Children );
			children.Compile();
		}

		public override void Draw ( ICommandBuffer commands ) {
			Source.maskingData.Push( new() {
				ToMaskingSpace = new( GlobalToUnit ),
				CornerExponents = CornerExponents,
				CornerRadii = CornerRadii
			} );
			children.Draw( commands );
			Source.maskingData.Pop();
		}

		public override void ReleaseResources ( bool willBeReused ) {
			children.Clear();
		}
	}

	string IViewableInDrawVisualiser.Name => $"{GetType().Name} ({Size}) [{Children.Count} Child{(Children.Count == 1 ? "" : "ren")}]";

	public override string ToString () {
		return $"{GetType().Name} ({Size}) [{Children.Count} Child{(Children.Count == 1 ? "" : "ren")}]";
	}

	TChild IReadOnlyList<TChild>.this[int index] => internalChildren[index].Child;

	int IReadOnlyCollection<TChild>.Count => internalChildren.Count;

	IEnumerator<TChild> IEnumerable<TChild>.GetEnumerator () {
		var count = internalChildren.Count;
		for ( int i = 0; i < count; i++ ) {
			yield return internalChildren[i].Child;
		}
	}

	IEnumerator IEnumerable.GetEnumerator () {
		var count = internalChildren.Count;
		for ( int i = 0; i < count; i++ ) {
			yield return internalChildren[i].Child;
		}
	}
}

public interface ICompositeUIComponent<out T> : IUIComponent, IReadOnlyCompositeComponent<UIComponent, T> where T : UIComponent {
	IReadOnlyDependencyCache Dependencies { get; }

	void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations );
	void OnChildDrawNodesInvalidated ();

	new public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildAdded;
	new public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildRemoved;

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<UIComponent, T>, T>? IReadOnlyCompositeComponent<UIComponent, T>.ChildAdded {
		add => ChildAdded += value;
		remove => ChildAdded -= value;
	}

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<UIComponent, T>, T>? IReadOnlyCompositeComponent<UIComponent, T>.ChildRemoved {
		add => ChildRemoved += value;
		remove => ChildRemoved -= value;
	}

	IEnumerable<IViewableInDrawVisualiser> IViewableInDrawVisualiser.Children => (this as IReadOnlyCompositeComponent<UIComponent, T>).Children;
}

[Flags]
public enum LayoutInvalidations {
	None = 0,
	/// <summary>
	/// Some layout parameter of this component changed. This will force this component to recalculate its layout.
	/// </summary>
	Self = 1,
	/// <summary>
	/// Some layout parameter of this components children changed. This will force this component to update the layout of its children.
	/// </summary>
	Children = 2,
	/// <summary>
	/// The required size for this component has changed.
	/// </summary>
	RequiredSize = 4
}