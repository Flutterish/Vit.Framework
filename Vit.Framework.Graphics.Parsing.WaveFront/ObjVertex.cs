using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Parsing.WaveFront;

public struct ObjVertex {
	public Point4<float> Position;
	public Point3<float> TextureCoordinates;
	public Vector3<float> Normal;
}
