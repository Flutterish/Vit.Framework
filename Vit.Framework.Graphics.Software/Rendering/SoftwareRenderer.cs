using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Textures;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Rendering;

public abstract class SoftwareRenderer : DisposableObject, IRenderer {
	public GraphicsApi GraphicsApi { get; }
	public SoftwareRenderer ( GraphicsApi graphicsApi ) {
		GraphicsApi = graphicsApi;
		commandBuffer = new( this );
	}

	public virtual void WaitIdle () { }
	public abstract Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T>;
	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new SpirvCompiler( spirv ).Specialise( spirv.Type switch {
			ShaderPartType.Vertex => ExecutionModel.Vertex,
			ShaderPartType.Fragment => ExecutionModel.Fragment,
			_ => throw new ArgumentException( $"Unsupported shader type: {spirv.Type}", nameof( spirv ) )
		} );
	}
	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		return new ShaderSet( parts, vertexInput );
	}
	public virtual IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}
	public virtual IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}
	public virtual IStagingBuffer<T> CreateStagingBuffer<T> () where T : unmanaged {
		return new Buffer<T>();
	}

	public IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, PixelFormat format ) {
		if ( format == PixelFormat.Rgba8 ) {
			return new Texture<Rgba32>( size, format );
		}
		else if ( format == PixelFormat.D24S8ui ) {
			return new Texture<D24S8>( size, format );
		}
		else {
			throw new NotImplementedException();
		}
	}
	public IStagingTexture2D CreateStagingTexture ( Size2<uint> size, PixelFormat format ) {
		return (IStagingTexture2D)CreateDeviceTexture( size, format );
	}
	public ISampler CreateSampler () {
		return new Sampler();
	}
	public IFramebuffer CreateFramebuffer ( IEnumerable<ITexture2DView> attachments, IDeviceTexture2D? depthStencilAttachment = null ) {
		return new TargetImage( attachments, depthStencilAttachment );
	}

	SoftwareImmadiateCommandBuffer commandBuffer;
	public virtual IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}
}
