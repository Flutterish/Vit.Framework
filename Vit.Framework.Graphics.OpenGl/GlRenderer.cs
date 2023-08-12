using System.Numerics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Rendering;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
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

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderProgram( parts.Select( x => x as UnlinkedShader )! );
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

	public ITexture CreateTexture ( Size2<uint> size, Graphics.Rendering.Textures.PixelFormat format ) {
		return new Texture2D( size, format );
	}

	GlImmediateCommandBuffer immediateCommandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return immediateCommandBuffer;
	}

	public void Dispose () {
		
	}

	public IUniformSetPool CreateUniformSetPool ( uint size, UniformSetInfo type ) {
		throw new NotImplementedException();
	}
}
