using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Memory;

namespace Vit.Framework.TwoD.Rendering;

public abstract class DrawNode : DisposableObject {
	protected readonly int SubtreeIndex;
	protected DrawNode ( int subtreeIndex ) {
		SubtreeIndex = subtreeIndex;
	}

	/// <summary>
	/// [Update Thread] <br/>
	/// Sets the parameters required to draw this draw node if it is invalidated, and validates it. The parameters set here <strong>*can not*</strong> be mutated outside this method.
	/// </summary>
	/// <remarks>
	/// It is recommeded to use <see cref="DrawNodeInvalidations"/>.
	/// </remarks>
	public abstract void Update ();

	/// <summary>
	/// [Draw Thread] <br/>
	/// Creates required resources and draws the element represented by this draw node.
	/// </summary>
	public abstract void Draw ( ICommandBuffer commands );

	/// <summary>
	/// [Draw Thread] <br/>
	/// Releases any created resources. This might be called because:
	/// <list type="number">
	///		<item>The draw node is being disposed.</item>
	///		<item>The renderer will be changed.</item>
	///		<item>The draw node is in an incative branch of the draw tree.</item>
	/// </list>
	/// </summary>
	/// <param name="willBeReused">Whether this draw node will be used again. If <see langword="false"/>, the draw node is allowed to enter invalid state.</param>
	public abstract void ReleaseResources ( bool willBeReused );
	protected sealed override void Dispose ( bool disposing ) {
		ReleaseResources( willBeReused: false );
	}
}

public interface IHasDrawNodes<out T> where T : DrawNode {
	/// <summary>
	/// [Update Thread] <br/>
	/// Retreives and updates (via <see cref="DrawNode.Update"/>) the draw node at a given subtree index.
	/// </summary>
	/// <param name="subtreeIndex">The subtree index. Can be 0, 1 or 2 as draw node trees are stored in a triple buffer.</param>
	/// <returns>The up-to-date draw node at the given subtree index.</returns>
	/// <typeparam name="TSpecialisation">The type of renderer to specialise the draw node for, if possible.</typeparam>
	T GetDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation;

	/// <summary>
	/// [Draw Thread] <br/>
	/// Disposes of own draw nodes and any shared data, and that of any owned <see cref="IHasDrawNodes{T}"/>.
	/// </summary>
	/// <remarks>
	/// While this call is in progress, the update thread either does not have access to this component or is suspended.
	/// </remarks>
	void DisposeDrawNodes ();

	bool IsDisposed { get; }
}

public interface IHasCompositeDrawNodes<out T> where T : DrawNode {
	IReadOnlyList<IHasDrawNodes<T>> CompositeDrawNodeSources { get; }
}

public abstract class CompositeDrawNode<TSource, TNode, TSpecialisation> : DrawNode where TSource : IHasCompositeDrawNodes<TNode> where TNode : DrawNode where TSpecialisation : unmanaged, IRendererSpecialisation {
	protected readonly TSource Source;
	public CompositeDrawNode ( TSource source, int subtreeIndex ) : base( subtreeIndex ) {
		Source = source;
		ChildNodes = new( source.CompositeDrawNodeSources.Count );
	}

	/// <summary>
	/// Validates the child list invalidation.
	/// </summary>
	/// <returns><see langword="true"/> if the child list should be updated, <see langword="false"/> otherwise.</returns>
	protected abstract bool ValidateChildList ();

	protected RentedArray<TNode> ChildNodes;
	public override void Update () {
		if ( !ValidateChildList() )
			return;

		var count = Source.CompositeDrawNodeSources.Count;
		ChildNodes.Clear();
		ChildNodes.ReallocateStorage( count );
		for ( int i = 0; i < count; i++ ) {
			ChildNodes[i] = Source.CompositeDrawNodeSources[i].GetDrawNode<TSpecialisation>( SubtreeIndex );
		}
	}

	public override void Draw ( ICommandBuffer commands ) {
		foreach ( var i in ChildNodes.AsSpan() ) {
			i.Draw( commands );
		}
	}

	public override void ReleaseResources ( bool willBeReused ) {
		if ( willBeReused )
			return;

		ChildNodes.Clear();
		ChildNodes.Dispose();
	}
}