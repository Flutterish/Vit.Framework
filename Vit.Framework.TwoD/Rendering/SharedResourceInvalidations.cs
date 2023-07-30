namespace Vit.Framework.TwoD.Rendering;

/// <summary>
/// Represents a shared resource state that can be updated.
/// </summary>
/// <remarks>
/// Use this in order not to re-compute shared resources multiple times.
/// </remarks>
public struct SharedResourceInvalidations {
	public ulong Id;
	public ulong ComputedId;

	public void Invalidate () {
		unchecked { Id++; }
	}

	public SharedResourceUpload GetUpload () {
		return new SharedResourceUpload { Id = Id + 1 }; // +1 in order to make sure the default state is invalidated
	}
}

public struct SharedResourceUpload {
	public ulong Id;

	/// <summary>
	/// Validates the shared resource state.
	/// </summary>
	/// <returns><see langword="true"/> if the resources need to be recomputed, <see langword="false"/> if they are up-to-date.</returns>
	public bool Validate ( ref SharedResourceInvalidations resources ) {
		if ( resources.ComputedId != Id ) { // the Id can never be lower than the resources.ComputedId because that would mean you are drawing an older draw node than last time
			resources.ComputedId = Id;
			return true;
		}
		return false;
	}
}
