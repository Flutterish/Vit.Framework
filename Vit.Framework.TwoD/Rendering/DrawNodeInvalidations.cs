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
