using System.Numerics;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
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
	/// <summary>
	/// Creates a matrix such that (-1, -1) is mapped to bottom-left of the screen, (1, 1) to top-right.
	/// </summary>
	Matrix4<T> CreateNdcCorrectionMatrix<T> () where T : INumber<T>;

	/// <summary>
	/// Creates a matrix such that (-1, -1) is mapped to (0, 0) of a framebuffer texture, (1, 1) to (1, 1) of a framebuffer texture.
	/// </summary>
	Matrix4<T> CreateUvCorrectionMatrix<T> () where T : INumber<T>;

	IShaderPart CompileShaderPart ( SpirvBytecode spirv );
	IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput );

	IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged;
	IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged;
	IStagingBuffer<T> CreateStagingBuffer<T> () where T : unmanaged; // TODO also add non-mutable buffers

	IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, PixelFormat format );
	IStagingTexture2D CreateStagingTexture ( Size2<uint> size, PixelFormat format );
	ISampler CreateSampler ();
	IFramebuffer CreateFramebuffer ( IEnumerable<IDeviceTexture2D> attachments, IDeviceTexture2D? depthStencilAttachment = null );

	IImmediateCommandBuffer CreateImmediateCommandBuffer (); // TODO currently all implementations behave as if the commands finished when disposed. this will not be true in the future
}
