using System.Numerics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11;

public class Direct3D11Renderer : DisposableObject, IRenderer {
	public readonly ID3D11Device Device;
	public readonly ID3D11DeviceContext Context;
	public readonly ID3D11RasterizerState RasterizerState;
	public Direct3D11Renderer ( Direct3D11Api graphicsApi, ID3D11Device device, ID3D11DeviceContext context ) {
		GraphicsApi = graphicsApi;
		Device = device;
		Context = context;
		commandBuffer = new( this, Context );

		RasterizerState = device.CreateRasterizerState( new() {
			FillMode = FillMode.Solid,
			CullMode = CullMode.None
		} );
		context.RSSetState( RasterizerState );
	}

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		throw new NotImplementedException();
	}

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new UnlinkedShader( spirv, Device );
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

	Dictionary<(BufferTest, DepthState), ID3D11DepthStencilState> depthStencilStates = new();
	public ID3D11DepthStencilState GetDepthStencilState ( BufferTest depth, DepthState state ) {
		var key = (depth, state);
		if ( !depthStencilStates.TryGetValue( key, out var depthStencilState ) ) {
			depthStencilStates.Add( key, depthStencilState = Device.CreateDepthStencilState( new() {
				DepthEnable = depth.IsEnabled,
				DepthWriteMask = state.WriteOnPass ? DepthWriteMask.All : DepthWriteMask.Zero,
				DepthFunc = depth.CompareOperation switch {
					CompareOperation.LessThan => ComparisonFunction.Less,
					CompareOperation.GreaterThan => ComparisonFunction.Greater,
					CompareOperation.Equal => ComparisonFunction.Equal,
					CompareOperation.NotEqual => ComparisonFunction.NotEqual,
					CompareOperation.LessThanOrEqual => ComparisonFunction.LessEqual,
					CompareOperation.GreaterThanOrEqual => ComparisonFunction.GreaterEqual,
					CompareOperation.Always => ComparisonFunction.Always,
					CompareOperation.Never or _ => ComparisonFunction.Never
				}
			} ) );
		}

		return depthStencilState;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in depthStencilStates ) {
			i.Dispose();
		}

		RasterizerState.Dispose();
		Device.Dispose();
		Context.Dispose();
	}

	public IUniformSet CreateUniformSet ( UniformSetInfo info ) {
		throw new NotImplementedException();
	}
}
