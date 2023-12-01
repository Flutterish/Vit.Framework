using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Memory;

namespace Vit.Framework.TwoD.Rendering;

public abstract class DrawNode : DisposableObject {
	public virtual DrawNodeBatchContract BatchContract { get => NullDrawNodeBatchContract.Instance; }
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
	/// <remarks>
	/// Usually only called when <see cref="BatchContract"/> is <see cref="NullDrawNodeBatchContract"/>.
	/// </remarks>
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
	/// Populates the <paramref name="collection"/> with updated (via <see cref="DrawNode.Update"/>) draw nodes at a given subtree index.
	/// </summary>
	/// <param name="subtreeIndex">The subtree index. Can be 0, 1 or 2 as draw node trees are stored in a triple buffer.</param>
	/// <param name="collection">The collection to populate.</param>
	/// <typeparam name="TSpecialisation">The type of renderer to specialise the draw node for, if possible.</typeparam>
	void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection ) where TSpecialisation : unmanaged, IRendererSpecialisation;

	/// <summary>
	/// [Draw Thread] <br/>
	/// Disposes of own draw nodes and any shared data, and that of any owned <see cref="IHasDrawNodes{T}"/>.
	/// </summary>
	/// <remarks>
	/// While this call is in progress, the update thread either does not have access to this component or is suspended.
	/// </remarks>
	void DisposeDrawNodes ();
}
