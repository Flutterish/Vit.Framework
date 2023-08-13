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

	public ITexture CreateTexture ( Size2<uint> size, PixelFormat format ) {
		return new Texture( size, format );
	}

	SoftwareImmadiateCommandBuffer commandBuffer;
	public virtual IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}
}
