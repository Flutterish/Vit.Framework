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

	/// <summary>
	/// Creates a host buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>bytes</b>. Must be greater than 0.</param>
	IHostBuffer<T> CreateHostBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged;
	/// <summary>
	/// Creates a device buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>bytes</b>. Must be greater than 0.</param>
	IDeviceBuffer<T> CreateDeviceBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged;
	/// <summary>
	/// Creates a staging buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>bytes</b>. Must be greater than 0.</param>
	IStagingBuffer<T> CreateStagingBufferRaw<T> ( uint size, BufferUsage usage ) where T : unmanaged; // TODO also add non-mutable buffers

	IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, PixelFormat format );
	IStagingTexture2D CreateStagingTexture ( Size2<uint> size, PixelFormat format );
	ISampler CreateSampler ();
	IFramebuffer CreateFramebuffer ( IEnumerable<IDeviceTexture2D> attachments, IDeviceTexture2D? depthStencilAttachment = null );

	IImmediateCommandBuffer CreateImmediateCommandBuffer (); // TODO currently all implementations behave as if the commands finished when disposed. this will not be true in the future
}

public static class IRendererExtensions {
	/// <summary>
	/// Creates a host buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IHostBuffer<T> CreateHostBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return renderer.CreateHostBufferRaw<T>( size * IBuffer<T>.Stride, type, usage );
	}

	/// <summary>
	/// Creates a device buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IDeviceBuffer<T> CreateDeviceBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return renderer.CreateDeviceBufferRaw<T>( size * IBuffer<T>.Stride, type, usage );
	}

	/// <summary>
	/// Creates a staging buffer.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IStagingBuffer<T> CreateStagingBuffer<T> ( this IRenderer renderer, uint size, BufferUsage usage ) where T : unmanaged {
		return renderer.CreateStagingBufferRaw<T>( size * IBuffer<T>.Stride, usage );
	}

	/// <summary>
	/// Creates a host buffer such that the size is a multiple of the alignment.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IHostBuffer<T> CreateAlignedHostBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage, uint alignment ) where T : unmanaged {
		var stride = (IBuffer<T>.Stride + alignment - 1) / alignment * alignment;
		return renderer.CreateHostBufferRaw<T>( (size - 1) * stride + IBuffer<T>.Stride, type, usage );
	}

	/// <summary>
	/// Creates a device buffer such that the size is a multiple of the alignment.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IDeviceBuffer<T> CreateAlignedDeviceBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage, uint alignment ) where T : unmanaged {
		var stride = (IBuffer<T>.Stride + alignment - 1) / alignment * alignment;
		return renderer.CreateDeviceBufferRaw<T>( (size - 1) * stride + IBuffer<T>.Stride, type, usage );
	}

	/// <summary>
	/// Creates a staging buffer such that the size is a multiple of the alignment.
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IStagingBuffer<T> CreateAlignedStagingBuffer<T> ( this IRenderer renderer, uint size, BufferUsage usage, uint alignment ) where T : unmanaged {
		var stride = (IBuffer<T>.Stride + alignment - 1) / alignment * alignment;
		return renderer.CreateStagingBufferRaw<T>( (size - 1) * stride + IBuffer<T>.Stride, usage );
	}

	/// <summary>
	/// Creates a host buffer such that its suitable to hold uniform data (aligned to 256 by default).
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IHostBuffer<T> CreateUniformHostBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage, uint alignment = 256 ) where T : unmanaged {
		return renderer.CreateAlignedHostBuffer<T>( size, type, usage, alignment );
	}

	/// <summary>
	/// Creates a device buffer such that its suitable to hold uniform data (aligned to 256 by default).
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IDeviceBuffer<T> CreateUniformDeviceBuffer<T> ( this IRenderer renderer, uint size, BufferType type, BufferUsage usage, uint alignment = 256 ) where T : unmanaged {
		return renderer.CreateAlignedDeviceBuffer<T>( size, type, usage, alignment );
	}

	/// <summary>
	/// Creates a staging buffer such that its suitable to hold uniform data (aligned to 256 by default).
	/// </summary>
	/// <typeparam name="T">The structure the buffer will hold.</typeparam>
	/// <param name="size">Size of the buffer in <b>elements</b>. Must be greater than 0.</param>
	public static IStagingBuffer<T> CreateUniformStagingBuffer<T> ( this IRenderer renderer, uint size, BufferUsage usage, uint alignment = 256 ) where T : unmanaged {
		return renderer.CreateAlignedStagingBuffer<T>( size, usage, alignment );
	}
}