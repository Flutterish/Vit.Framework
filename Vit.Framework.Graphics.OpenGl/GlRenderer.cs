﻿using System.Numerics;
using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Rendering;
using Vit.Framework.Graphics.OpenGl.Shaders;
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
		return new Shader( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderProgram( parts.Select( x => x as Shader )! );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>( type switch {
			BufferType.Vertex => BufferTarget.ArrayBuffer,
			BufferType.Index => BufferTarget.ArrayBuffer,
			BufferType.Uniform => BufferTarget.UniformBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return (Buffer<T>)CreateHostBuffer<T>( type );
	}

	GlImmediateCommandBuffer immediateCommandBuffer = new();
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return immediateCommandBuffer;
	}

	public void Dispose () {
		
	}
}
