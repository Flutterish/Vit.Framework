using System.Numerics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11;

public class Direct3D11Renderer : DisposableObject, IRenderer {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public Direct3D11Renderer ( Direct3D11Api graphicsApi, ID3D11Device device, ID3D11DeviceContext context ) {
		GraphicsApi = graphicsApi;
		Device = device;
		Context = context;
		commandBuffer = new( Context );
	}

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		throw new NotImplementedException();
	}

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return spirv.Type switch {
			ShaderPartType.Vertex => new VertexShader( spirv, Device ),
			ShaderPartType.Fragment => new PixelShader( spirv, Device ),
			_ => throw new ArgumentException( $"Unsupported shader type: {spirv.Type}", nameof( spirv ) )
		};
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderSet( parts, Context );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new Buffer<T>( Device, Context, type switch {
			BufferType.Vertex => BindFlags.VertexBuffer,
			BufferType.Index => BindFlags.IndexBuffer,
			BufferType.Uniform => BindFlags.ConstantBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return (Buffer<T>)CreateHostBuffer<T>( type );
	}

	public ITexture CreateTexture ( Size2<uint> size, PixelFormat format ) {
		return new Texture2D( Device, size, format );
	}

	Direct3D11ImmediateCommandBuffer commandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}

	protected override void Dispose ( bool disposing ) {
		Device.Dispose();
		Context.Dispose();
	}
}
