using System.Numerics;
using Vit.Framework.Graphics.Curses.Buffers;
using Vit.Framework.Graphics.Curses.Rendering;
using Vit.Framework.Graphics.Curses.Shaders;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Curses;

public class CursesRenderer : DisposableObject, IRenderer {
	public GraphicsApi GraphicsApi { get; }
	public CursesRenderer ( CursesApi graphicsApi ) {
		GraphicsApi = graphicsApi;
	}


	public void WaitIdle () {
		throw new NotImplementedException();
	}

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		throw new NotImplementedException();
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new Shader( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderSet( parts );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>();
	}

	CursesImmadiateCommandBuffer commandBuffer = new();
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}

	protected override void Dispose ( bool disposing ) {
		
	}
}
