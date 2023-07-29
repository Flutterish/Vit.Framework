using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Memory;

namespace Vit.Framework.TwoD.Rendering;

/// <summary>
/// Stores bitfield flags for 2 sets of 3 draw node invalidations. When zero-initialized, it represents all draw nodes being invalidated.
/// </summary>
public struct DrawNodeInvalidations {
	byte validationBitField;

	/// <summary>
	/// Invalidates all draw nodes.
	/// </summary>
	/// <returns><see langword="true"/> if any draw node was invalidated, <see langword="false"/> otherwise.</returns>
	public bool InvalidateDrawNodes () {
		if ( validationBitField == 0b_000_000 )
			return false;

		validationBitField = 0b_000_000;
		return true;
	}

	/// <summary>
	/// Performs <see cref="InvalidateDrawNodes"/> on just the first invalidation set.
	/// </summary>
	public bool InvalidateFirstSet () {
		if ( (validationBitField & 0b000_111) == 0b_000_000 )
			return false;

		validationBitField &= 0b_111_000;
		return true;
	}

	/// <summary>
	/// Performs <see cref="InvalidateDrawNodes"/> on just the second invalidation set.
	/// </summary>
	public bool InvalidateSecondSet () {
		if ( (validationBitField & 0b111_000) == 0b_000_000 )
			return false;

		validationBitField &= 0b_000_111;
		return true;
	}

	/// <summary>
	/// Validates a draw node at a given subtree index. Add 3 to access the second invalidation set.
	/// </summary>
	/// <param name="index">The index of the draw node to validate.</param>
	/// <returns><see langword="true"/> if the node was validated, <see langword="false"/> if it was already valid.</returns>
	public bool ValidateDrawNode ( int index ) {
		var mask = 1 << index;
		if ( (validationBitField & mask) != 0 )
			return false;

		validationBitField |= (byte)mask;
		return true;
	}


	public bool IsInvalidated ( int index ) {
		var mask = 1 << index;
		if ( (validationBitField & mask) != 0 )
			return false;

		return true;
	}

	public override string ToString () {
		var self = this;
		char v ( int i ) => self.IsInvalidated( i ) ? '-' : '+';
		return $"Set 1: [{v( 0 )}{v( 1 )}{v( 2 )}] Set2: [{v( 4 )}{v( 5 )}{v( 6 )}]";
	}
}

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
	T GetDrawNode ( int subtreeIndex );

	/// <summary>
	/// [Draw Thread] <br/>
	/// Disposes of owned draw nodes and any shared data.
	/// </summary>
	void DisposeDrawNodes ();
}

public interface IHasCompositeDrawNodes<out T> where T : DrawNode {
	IReadOnlyList<IHasDrawNodes<T>> CompositeDrawNodeSources { get; }
}

public abstract class CompositeDrawNode<TSource, TNode> : DrawNode where TSource : IHasCompositeDrawNodes<TNode> where TNode : DrawNode {
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
			ChildNodes[i] = Source.CompositeDrawNodeSources[i].GetDrawNode( SubtreeIndex );
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