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

namespace Vit.Framework.TwoD.UI;

public abstract class CompositeUIComponent : CompositeUIComponent<UIComponent> { }
public abstract class CompositeUIComponent<T> : UIComponent, ICompositeUIComponent<T>, IHasCompositeDrawNodes<DrawNode> where T : UIComponent {
	List<T> internalChildren = new();
	public IReadOnlyList<T> Children {
		get => internalChildren;
		protected init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}

	public T InternalChild {
		get => internalChildren.Single();
		protected init => AddInternalChild( value );
	}

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

	protected void AddInternalChild ( T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = internalChildren.Count;
		internalChildren.Add( child );
		if ( IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}
	protected void InsertInternalChild ( int index, T child ) {
		if ( child.Parent != null )
			throw new InvalidOperationException( "Components may only have 1 parent" );

		child.Parent = this;
		child.Depth = index;
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Depth++;
		}
		internalChildren.Insert( index, child );
		if ( IsLoaded )
			child.Load( Dependencies );
		onChildAdded( child );
	}

	protected void RemoveInternalChild ( T child ) {
		var index = child.Depth;
		if ( internalChildren[index] != child )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		RemoveInternalChildAt( index );
	}
	protected void RemoveInternalChildAt ( int index ) {
		var child = internalChildren[index];

		child.Parent = null;
		internalChildren.RemoveAt( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Depth--;
		}
		if ( child.IsLoaded )
			child.Unload();
		onChildRemoved( child );
	}

	protected void NoUnloadRemoveInternalChild ( T child ) {
		var index = child.Depth;
		if ( internalChildren[index] != child )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		NoUnloadRemoveInternalChildAt( index );
	}
	protected void NoUnloadRemoveInternalChildAt ( int index ) {
		var child = internalChildren[index];

		child.Parent = null;
		internalChildren.RemoveAt( index );
		for ( int i = index; i < internalChildren.Count; i++ ) {
			internalChildren[i].Depth--;
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
			var child = internalChildren[^1];
			RemoveInternalChildAt( internalChildren.Count - 1 );
			child.Dispose( disposeScheduler );
		}
	}

	void onChildAdded ( T child ) {
		child.EventHandlerAdded += onChildEventHandlerAdded;
		child.EventHandlerRemoved += onChildEventHandlerRemoved;
		foreach ( var (type, tree) in child.HandledEventTypes ) {
			onChildEventHandlerAdded( type, tree );
		}
		ChildAdded?.Invoke( this, child );
		InvalidateLayout( LayoutInvalidations.Children );
		invalidateDrawNodes();
	}

	void onChildRemoved ( T child ) {
		child.EventHandlerAdded -= onChildEventHandlerAdded;
		child.EventHandlerRemoved -= onChildEventHandlerRemoved;
		foreach ( var (type, tree) in child.HandledEventTypes ) {
			onChildEventHandlerRemoved( type, tree );
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
		tree.Sort( static ( a, b ) => a.Source.Depth - b.Source.Depth );
	}

	protected virtual IReadOnlyDependencyCache CreateDependencies ( IReadOnlyDependencyCache parent ) {
		return parent;
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		maskingData = dependencies.Resolve<MaskingDataBuffer>();
		Dependencies = CreateDependencies( dependencies );
		foreach ( var i in internalChildren ) {
			i.Load( Dependencies );
		}
	}
	protected override void OnUnload () {
		foreach ( var i in internalChildren.Reverse<T>() ) {
			i.Unload();
		}
		Dependencies = null!;
	}

	public override void Update () {
		base.Update();
		updateSubtree();
	}

	void updateSubtree () {
		foreach ( var i in internalChildren ) {
			i.Update();
		}
	}

	protected override void OnMatrixInvalidated () {
		foreach ( var i in internalChildren ) {
			i.OnParentMatrixInvalidated();
		}
	}

	public virtual void OnChildLayoutInvalidated ( UIComponent child, LayoutInvalidations invalidations ) {
		InvalidateLayout( LayoutInvalidations.Children );
	}

	protected sealed override void PerformLayout () {
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Self ) ) {
			PerformSelfLayout();
		}
		if ( LayoutInvalidations.HasFlag( LayoutInvalidations.Children ) ) {
			foreach ( var i in Children ) {
				i.ComputeLayout();
			}
		}
	}

	protected virtual void PerformSelfLayout () { }

	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildAdded;
	public event HierarchyObserver.ChildObserver<ICompositeUIComponent<T>, T>? ChildRemoved;

	void invalidateDrawNodes () {
		if ( drawNodeInvalidations.InvalidateDrawNodes() )
			Parent?.OnChildDrawNodesInvalidated();
	}
	public void OnChildDrawNodesInvalidated () {
		invalidateDrawNodes();
	}
	DrawNodeInvalidations drawNodeInvalidations;
	DrawNode?[] drawNodes = new DrawNode?[3];
	public override Rendering.DrawNode GetDrawNode<TSpecialisation> ( int subtreeIndex ) {
		if ( internalChildren.Count == 1 && !isMaskingActive ) {
			drawNodeInvalidations.ValidateDrawNode( subtreeIndex );
			return internalChildren[0].GetDrawNode<TSpecialisation>( subtreeIndex );
		}

		var node = drawNodes[subtreeIndex] ??= CreateDrawNode<TSpecialisation>( subtreeIndex );
		node.Update();
		return node;
	}

	protected virtual Rendering.DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		return new DrawNode<TSpecialisation>( this, subtreeIndex );
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
	public class DrawNode<TSpecialisation> : CompositeDrawNode<CompositeUIComponent<T>, Rendering.DrawNode, TSpecialisation> where TSpecialisation : unmanaged, IRendererSpecialisation {
		public DrawNode ( CompositeUIComponent<T> source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override bool ValidateChildList () {
			return Source.drawNodeInvalidations.ValidateDrawNode( SubtreeIndex );
		}

		protected bool IsMaskingActive;
		protected Corners<float> CornerExponents;
		protected Corners<Axes2<float>> CornerRadii;
		protected Matrix3<float> GlobalToUnit;
		public override void Update () {
			base.Update();

			IsMaskingActive = Source.isMaskingActive;
			if ( IsMaskingActive ) {
				GlobalToUnit = Source.GlobalToUnitMatrix * Matrix3<float>.CreateScale( 1f / Source.Width, 1f / Source.Height );
				CornerExponents = Source.cornerExponents;
				CornerRadii = MaskingData.NormalizeCornerRadii( Source.cornerRadii, Source.Size );
			}
		}

		public override void Draw ( ICommandBuffer commands ) {
			if ( IsMaskingActive ) {
				Source.maskingData.Push( new() {
					ToMaskingSpace = new( GlobalToUnit ),
					CornerExponents = CornerExponents,
					CornerRadii = CornerRadii
				} );
				base.Draw( commands );
				Source.maskingData.Pop();
			}
			else {
				base.Draw( commands );
			}
		}
	}

	public IReadOnlyList<IHasDrawNodes<Rendering.DrawNode>> CompositeDrawNodeSources => internalChildren;
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