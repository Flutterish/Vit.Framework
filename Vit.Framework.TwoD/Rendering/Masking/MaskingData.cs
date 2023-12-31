﻿using Vit.Framework.Mathematics;
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
	/// A matrix that transforms global space (the space resulting from applying the model matrix) such that the whole mask is mapped to a [0;1] range.
	/// </summary>
	public required Matrix4x3<float> ToMaskingSpace;
	/// <summary>
	/// Radius of each corner respectively, in a [0;1] range, where 0 is no rounded corner at all and 1 is a rounded corner reaching to the center of the mask.
	/// </summary>
	public required Corners<Axes2<float>> CornerRadii;
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

	public static Corners<Axes2<float>> NormalizeCornerRadii ( Corners<Axes2<float>> radii, Size2<float> size ) {
		var inverseX = 2f / size.Width;
		var inverseY = 2f / size.Height;
		Axes2<float> normalize ( Axes2<float> radii ) {
			return new() {
				X = float.Clamp( radii.X * inverseX, 0, 1 ),
				Y = float.Clamp( radii.Y * inverseY, 0, 1 )
			};
		}

		return new() {
			TopLeft = normalize( radii.TopLeft ),
			TopRight = normalize( radii.TopRight ),
			BottomLeft = normalize( radii.BottomLeft ),
			BottomRight = normalize( radii.BottomRight )
		};
	}
}
