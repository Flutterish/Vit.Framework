using Vit.Framework.Interop;

namespace Vit.Framework.TwoD.Rendering.Masking;

/// <summary>
/// A masking instruction. This can be used to create complex masks.
/// </summary>
public struct MaskingInstruction { // 16B
	public required Instruction Instruction;
	/// <summary>
	/// Pointer into the masking data buffer, which specify the instructions to use as arguments. 
	/// This forms a linked list of all masking instrcutions to apply.
	/// </summary>
	public FixedSpan3<uint> Args;
}

public enum Instruction : uint {
	/// <summary>
	/// Returns a boolean result of the test performed using the <see cref="MaskingData"/> that precedes this instruction in the masking buffer. Arguments do not apply to this instruction.
	/// </summary>
	/// <remarks>
	/// The <see cref="MaskingData"/> has to preceed it because <c>0</c> is a <see langword="null"/> pointer for instructions, but not <see cref="MaskingData"/>. This way, the whole masking buffer can used.
	/// </remarks>
	Test = 1,
	/// <summary>
	/// Returns <see langword="true"/>.
	/// </summary>
	ConstantTrue = 2,
	/// <summary>
	/// returns <see langword="false"/>.
	/// </summary>
	ConstantFalse = 3,
	/// <summary>
	/// Performs a set intersection on all arguments. This is equivalent to <c>A &amp;&amp; B</c>.
	/// </summary>
	Intersect = 4,
	/// <summary>
	/// Performs a set union on all arguments. This is equivalent to <c>A || B</c>.
	/// </summary>
	Union = 5,
	/// <summary>
	/// Inverts the first argument. This is equivalent to <c>!A</c>.
	/// </summary>
	Invert = 6,
	/// <summary>
	/// Persorms a set difference between the first argument and the rest. This is equivalent to <c>A &amp;&amp; !B</c>.
	/// </summary>
	Substract = 7,
	/// <summary>
	/// Persorms a symmetric difference between all the aguments. This is equivalent to <c>A != B</c>.
	/// </summary>
	SymmetricDifference = 8,
	/// <summary>
	/// If the first argument is <see langword="true"/>, returns the second argument, otherwise returns the third argument. This is equicalentt to <c>A ? B : C</c>.
	/// </summary>
	Branch = 9
}
