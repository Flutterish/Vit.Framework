using System.Diagnostics.CodeAnalysis;

namespace Vit.Framework.Graphics.Rendering;

public struct BufferTest {
	public required bool IsEnabled;
	public CompareOperation CompareOperation;

	[SetsRequiredMembers]
	public BufferTest ( CompareOperation compareOperation ) {
		IsEnabled = true;
		CompareOperation = compareOperation;
	}
}

public struct DepthState {
	public required bool WriteOnPass;
}

[Flags]
public enum CompareOperation {
	Never = 0x0,
	LessThan = 0x1,
	GreaterThan = 0x2,
	Equal = 0x4,

	NotEqual = LessThan | GreaterThan,
	LessThanOrEqual = LessThan | Equal,
	GreaterThanOrEqual = GreaterThan | Equal,
	Always = LessThan | Equal | GreaterThan
}

public struct StencilState { // TODO it seems there are separate functions for front and back faces
	/// <summary>
	/// The operation to perform when both the stencil and depth test pass.
	/// </summary>
	public StencilOperation PassOperation;
	/// <summary>
	/// The operation to perform when the stencil tests fails.
	/// </summary>
	public StencilOperation StencilFailOperation;
	/// <summary>
	/// The operation to perform when the stencil test passes, but the depth test fails.
	/// </summary>
	public StencilOperation DepthFailOperation;

	[SetsRequiredMembers]
	public StencilState ( StencilOperation pass, StencilOperation fail = StencilOperation.Keep, StencilOperation depthFail = StencilOperation.Keep ) {
		CompareMask = WriteMask = ~0u;
		PassOperation = pass;
		StencilFailOperation = fail;
		DepthFailOperation = depthFail;
	}

	public required uint CompareMask;
	public required uint WriteMask;
	public uint ReferenceValue;
}

public enum StencilOperation {
	/// <summary>
	/// Keep the current value.
	/// </summary>
	Keep,
	/// <summary>
	/// Set the value to 0.
	/// </summary>
	SetTo0,
	/// <summary>
	/// Replace the value with the reference value.
	/// </summary>
	ReplaceWithReference,
	/// <summary>
	/// Bitwise invert the current value.
	/// </summary>
	Invert,
	/// <summary>
	/// Increment the value, while preventing it from overflowing.
	/// </summary>
	Increment,
	/// <summary>
	/// Decrements the value, while preventing it from underflowing.
	/// </summary>
	Decrement,
	/// <summary>
	/// Increments the value, allowing it to overflow.
	/// </summary>
	IncrementWithWrap,
	/// <summary>
	/// Decrements the value, allowing it to underflow.
	/// </summary>
	DecrementWithWrap
}