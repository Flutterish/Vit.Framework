using System.Numerics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Rendering;

public abstract class SoftwareRenderer : DisposableObject, IRenderer {
	public GraphicsApi GraphicsApi { get; }
	public SoftwareRenderer ( GraphicsApi graphicsApi ) {
		GraphicsApi = graphicsApi;
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
	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderSet( parts );
	}
	public virtual IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}
	public virtual IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}

	SoftwareImmadiateCommandBuffer commandBuffer = new();
	public virtual IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}
}
