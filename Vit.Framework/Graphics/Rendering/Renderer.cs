using System.Numerics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A specialised usage of a <see cref="GraphicsApi"/> capable of rendering graphics on a specific logical device.
/// </summary>
/// <remarks>
/// This is equivalent to <c>VkDevice</c>, an OpenGL context or <c>ID3DXDevice</c>.
/// </remarks>
public abstract class Renderer : DisposableObject {
	public readonly GraphicsApi GraphicsApi;

	public Renderer ( GraphicsApi api ) {
		GraphicsApi = api;
	}

	public abstract void WaitIdle ();

	public abstract Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : unmanaged, INumber<T>;
}