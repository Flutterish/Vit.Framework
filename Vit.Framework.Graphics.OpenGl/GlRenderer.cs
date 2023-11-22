using System.Numerics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Rendering;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.OpenGl;

public class GlRenderer : IRenderer {
	public GlRenderer ( OpenGlApi graphicsApi ) {
		GraphicsApi = graphicsApi;
	}

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		GL.Finish();
	}

	public Matrix4<T> CreateNdcCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}
	public Matrix4<T> CreateUvCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new UnlinkedShader( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		return new ShaderProgram( parts.Select( x => (x as UnlinkedShader)! ), vertexInput );
	}

	public IHostBuffer<T> CreateHostBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return new HostBuffer<T>( size, type switch {
			BufferType.Vertex => BufferTarget.ArrayBuffer,
			BufferType.Index => BufferTarget.ArrayBuffer,
			BufferType.Uniform => BufferTarget.UniformBuffer,
			BufferType.ReadonlyStorage => BufferTarget.ShaderStorageBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof( type ) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return new DeviceBuffer<T>( size, type switch {
			BufferType.Vertex => BufferTarget.ArrayBuffer,
			BufferType.Index => BufferTarget.ArrayBuffer,
			BufferType.Uniform => BufferTarget.UniformBuffer,
			BufferType.ReadonlyStorage => BufferTarget.ShaderStorageBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof( type ) )
		} );
	}

	public IStagingBuffer<T> CreateStagingBufferRaw<T> ( uint size, BufferUsage usage ) where T : unmanaged {
		return new HostBuffer<T>( size, BufferTarget.CopyReadBuffer );
	}

	public IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		return new Texture2DStorage( size, format ); // TODO use renderbuffers for depth/stencil
	}

	public IStagingTexture2D CreateStagingTexture ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		return new PixelBuffer( size, format );
	}

	public ISampler CreateSampler () {
		return new Sampler();
	}

	public IFramebuffer CreateFramebuffer ( IEnumerable<IDeviceTexture2D> attachments, IDeviceTexture2D? depthStencilAttachment = null ) {
		return new FrameBuffer( attachments, depthStencilAttachment );
	}

	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return new GlImmediateCommandBuffer( this ); // TODO instead of creating a new one, we can pool them
	}

	public void Dispose () {
		
	}
}
