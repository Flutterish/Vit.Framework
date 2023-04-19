using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Rendering;

public abstract class Renderer {
	public readonly GraphicsApi GraphicsApi;

	public Renderer ( GraphicsApi api ) {
		GraphicsApi = api;
	}

	public abstract Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : unmanaged, INumber<T>;
}
