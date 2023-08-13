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
	Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T>;

	IShaderPart CompileShaderPart ( SpirvBytecode spirv );
	IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput );

	IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged;
	IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged;
	//IStagingBuffer<T> CreateStagingBuffer<T> () where T : unmanaged; // TODO also add non-mutable buffers

	ITexture CreateTexture ( Size2<uint> size, PixelFormat format );

	IImmediateCommandBuffer CreateImmediateCommandBuffer ();
}