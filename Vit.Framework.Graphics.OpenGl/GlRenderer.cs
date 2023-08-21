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
		immediateCommandBuffer = new( this );
	}

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		GL.Finish();
	}

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new UnlinkedShader( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		return new ShaderProgram( parts.Select( x => (x as UnlinkedShader)! ), vertexInput );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new HostBuffer<T>( type switch {
			BufferType.Vertex => BufferTarget.ArrayBuffer,
			BufferType.Index => BufferTarget.ArrayBuffer,
			BufferType.Uniform => BufferTarget.UniformBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new DeviceBuffer<T>( type switch {
			BufferType.Vertex => BufferTarget.ArrayBuffer,
			BufferType.Index => BufferTarget.ArrayBuffer,
			BufferType.Uniform => BufferTarget.UniformBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof( type ) )
		} );
	}

	public IStagingBuffer<T> CreateStagingBuffer<T> () where T : unmanaged {
		return new HostBuffer<T>( BufferTarget.CopyReadBuffer );
	}

	public ITexture2D CreateTexture ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		return new Texture2D( size, format ); // TODO use renderbuffers for depth/stencil
	}

	GlImmediateCommandBuffer immediateCommandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return immediateCommandBuffer;
	}

	public void Dispose () {
		
	}
}
