using System.Numerics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.OpenGl;

public class GlRenderer : IRenderer {
	public GlRenderer ( OpenGlApi graphicsApi ) {
		GraphicsApi = graphicsApi;
	}

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		throw new NotImplementedException();
	}

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		throw new NotImplementedException();
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		throw new NotImplementedException();
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		throw new NotImplementedException();
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		throw new NotImplementedException();
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		throw new NotImplementedException();
	}

	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		throw new NotImplementedException();
	}

	public void Dispose () {
		throw new NotImplementedException();
	}
}
