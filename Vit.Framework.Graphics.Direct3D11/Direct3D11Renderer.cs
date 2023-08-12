using System.Numerics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Direct3D11.Uniforms;
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
using StencilOp = Vortice.Direct3D11.StencilOperation;
using StencilOperation = Vit.Framework.Graphics.Rendering.StencilOperation;

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

	public IUniformSetPool CreateUniformSetPool ( uint size, UniformSetInfo type ) {
		return new UniformSetPool( type );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new HostBuffer<T>( Device, Context, type switch {
			BufferType.Vertex => BindFlags.VertexBuffer,
			BufferType.Index => BindFlags.IndexBuffer,
			BufferType.Uniform => BindFlags.ConstantBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new DeviceBuffer<T>( Device, Context, type switch {
			BufferType.Vertex => BindFlags.VertexBuffer,
			BufferType.Index => BindFlags.IndexBuffer,
			BufferType.Uniform => BindFlags.ConstantBuffer,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof( type ) )
		} );
	}

	public ITexture CreateTexture ( Size2<uint> size, PixelFormat format ) {
		return new Texture2D( Device, size, format );
	}

	Direct3D11ImmediateCommandBuffer commandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return commandBuffer;
	}

	Dictionary<DepthStencilStateInfo, ID3D11DepthStencilState> depthStencilStates = new();
	public ID3D11DepthStencilState GetDepthStencilState ( DepthStencilStateInfo depthStencil ) {
		var key = depthStencil;
		if ( !depthStencilStates.TryGetValue( key, out var depthStencilState ) ) {
			static StencilOp stencilOp ( StencilOperation operation ) {
				return operation switch {
					StencilOperation.SetTo0 => StencilOp.Zero,
					StencilOperation.ReplaceWithReference => StencilOp.Replace,
					StencilOperation.Invert => StencilOp.Invert,
					StencilOperation.Increment => StencilOp.IncrementSaturate,
					StencilOperation.Decrement => StencilOp.DecrementSaturate,
					StencilOperation.IncrementWithWrap => StencilOp.Increment,
					StencilOperation.DecrementWithWrap => StencilOp.Decrement,
					StencilOperation.Keep or _ => StencilOp.Keep
				};
			}

			static ComparisonFunction comparisonFunction ( CompareOperation operation ) {
				return operation switch {
					CompareOperation.LessThan => ComparisonFunction.Less,
					CompareOperation.GreaterThan => ComparisonFunction.Greater,
					CompareOperation.Equal => ComparisonFunction.Equal,
					CompareOperation.NotEqual => ComparisonFunction.NotEqual,
					CompareOperation.LessThanOrEqual => ComparisonFunction.LessEqual,
					CompareOperation.GreaterThanOrEqual => ComparisonFunction.GreaterEqual,
					CompareOperation.Always => ComparisonFunction.Always,
					CompareOperation.Never or _ => ComparisonFunction.Never
				};
			}

			DepthStencilOperationDescription stencil = new() {
				StencilFunc = comparisonFunction( depthStencil.StencilTest.CompareOperation ),
				StencilPassOp = stencilOp( depthStencil.StencilState.PassOperation ),
				StencilFailOp = stencilOp( depthStencil.StencilState.StencilFailOperation ),
				StencilDepthFailOp = stencilOp( depthStencil.StencilState.DepthFailOperation )
			};

			depthStencilStates.Add( key, depthStencilState = Device.CreateDepthStencilState( new() {
				DepthEnable = depthStencil.DepthTest.IsEnabled,
				DepthWriteMask = depthStencil.DepthState.WriteOnPass ? DepthWriteMask.All : DepthWriteMask.Zero,
				DepthFunc = comparisonFunction( depthStencil.DepthTest.CompareOperation ),
				StencilEnable = depthStencil.StencilTest.IsEnabled,
				StencilReadMask = (byte)depthStencil.StencilState.CompareMask,
				StencilWriteMask = (byte)depthStencil.StencilState.WriteMask,
				FrontFace = stencil,
				BackFace = stencil
			} ) );
		}

		return depthStencilState;
	}
	public struct DepthStencilStateInfo {
		public required BufferTest DepthTest;
		public required DepthState DepthState;
		public required BufferTest StencilTest;
		public required StencilState StencilState;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in depthStencilStates ) {
			i.Dispose();
		}

		RasterizerState.Dispose();
		Device.Dispose();
		Context.Dispose();
	}
}
