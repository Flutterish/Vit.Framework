using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Rendering.Masking;

/// <summary>
/// Data used for masking rectangles with rounded corners.
/// </summary>
/// <remarks>
/// <list type="table">
///		<item>A corner radius of 1 and exponent of 0 effectively cuts the corner out of the mask.</item>
///		<item>Combining cutting out corners and remapping the masking space matrix to the remaining quadrants, it is possible to create a mask shaped like just 1 or 2 corners.</item>
/// </list>
/// </remarks>
public struct MaskingData { // 96B (16 * 6)
	/// <summary>
	/// A matrix that transforms global space (the space resulting from applying the model matrix) such that:
	/// <list type="bullet">
	///		<item>The center of the mask is mapped to (0,0).</item>
	///		<item>The whole mask is mapped to a [-1;1] range.</item>
	/// </list>
	/// </summary>
	public required Matrix4x3<float> ToMaskingSpace;
	/// <summary>
	/// Radius of each corner respectively, in a [0;1] range.
	/// </summary>
	public required Corners<Vector2<float>> CornerRadii;
	/// <summary>
	/// Exponent of each corner respecitvely. The equation used is <c>|x|^exponent + |y|^exponent &lt;= 1</c> (adjusted for corner radius).
	/// <list type="table">
	///		<item>A value of 0 results in a completely cut-out corner.</item>
	///		<item>A value of 0-1 results in convave arcs.</item>
	///		<item>A value of 1 results in straight lines.</item>
	///		<item>A value of 1-2 results in bevels with sharp corners.</item>
	///		<item>A value of 2 results in circular arcs.</item>
	///		<item>A value of 2.5 results in something similar to apple's smooth corner.</item>
	///		<item>Higher values results in progressively more square squircles.</item>
	///		<item>We do not speak about negative values.</item>
	/// </list>
	/// </summary>
	/// <remarks>An interactive demo can be found <see href="https://www.desmos.com/calculator/bdnfusuk9o">here</see>.</remarks>
	public required Corners<float> CornerExponents;
}
