using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public interface IHas2DTransform {
	public Point2<float> Position { get; set; }
	public float X { get; set; }
	public float Y { get; set; }

	public Radians<float> Rotation { get; set; }

	public Point2<float> Origin { get; set; }
	public float OriginX { get; set; }
	public float OriginY { get; set; }

	public Axes2<float> Scale { get; set; }
	public float ScaleX { get; set; }
	public float ScaleY { get; set; }

	public Axes2<float> Shear { get; set; }
	public float ShearX { get; set; }
	public float ShearY { get; set; }

	/// <summary>
	/// A matrix such that (0,0) is mapped to the bottom left corner
	/// and (1,1) is mapped to the top right corner in global space.
	/// </summary>
	Matrix3<float> UnitToGlobalMatrix { get; }

	/// <summary>
	/// A matrix such that the bottom left corner in global space is mapped to (0,0)
	/// and the top right corner in global space is mapped to (1,1).
	/// </summary>
	Matrix3<float> GlobalToUnitMatrix { get; }

	public Point2<float> ScreenSpaceToLocalSpace ( Point2<float> point )
		=> GlobalToUnitMatrix.Apply( point );

	public Point2<float> LocalSpaceToScreenSpace ( Point2<float> point )
		=> UnitToGlobalMatrix.Apply( point );
}
