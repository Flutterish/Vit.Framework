using System.Numerics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// A specialised usage of a <see cref="GraphicsApi"/> capable of rendering graphics on a specific logical device.
/// </summary>
/// <remarks>
/// This is equivalent to <c>VkDevice</c>, an OpenGL context or <c>ID3DXDevice</c>.
/// </remarks>
public interface IRenderer : IDisposable {
	GraphicsApi GraphicsApi { get; }

	void WaitIdle ();
	Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T>;

	IShaderPart CompileShaderPart ( SpirvBytecode spirv );
	IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts );

	IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged;
	IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged;

	IImmediateCommandBuffer CreateImmediateCommandBuffer ();
}