using System.Numerics;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Textures;
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

		RasterizerState = device.CreateRasterizerState( new() {
			FillMode = FillMode.Solid,
			CullMode = CullMode.None
		} );
		context.RSSetState( RasterizerState );
	}

	public Direct3D11ImmediateCommandBuffer? ActiveImmediateContextState;
	public ID3D11RenderTargetView[]? ActiveImmediateContextFrameBufferAttachments;
	public ID3D11DepthStencilView? ActiveImmediateContextFrameBufferDepthStencil;

	public GraphicsApi GraphicsApi { get; }

	public void WaitIdle () {
		throw new NotImplementedException();
	}

	public Matrix4<T> CreateNdcCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}
	public Matrix4<T> CreateUvCorrectionMatrix<T> () where T : INumber<T> {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return new UnlinkedShader( spirv, Device );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		return new ShaderSet( parts, vertexInput );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new HostBuffer<T>( Device, Context, type switch {
			BufferType.Vertex => BindFlags.VertexBuffer,
			BufferType.Index => BindFlags.IndexBuffer,
			BufferType.Uniform => BindFlags.ConstantBuffer,
			BufferType.ReadonlyStorage => BindFlags.ShaderResource,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new DeviceBuffer<T>( Device, Context, type switch {
			BufferType.Vertex => BindFlags.VertexBuffer,
			BufferType.Index => BindFlags.IndexBuffer,
			BufferType.Uniform => BindFlags.ConstantBuffer,
			BufferType.ReadonlyStorage => BindFlags.ShaderResource,
			_ => throw new ArgumentException( $"Unsupported buffer type: {type}", nameof( type ) )
		} );
	}

	public IStagingBuffer<T> CreateStagingBuffer<T> () where T : unmanaged {
		return new StagingBuffer<T>( Device, Context );
	}

	public IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, PixelFormat format ) {
		return new Texture2D( Device, size, format, isStaging: false );
	}

	public IStagingTexture2D CreateStagingTexture ( Size2<uint> size, PixelFormat format ) {
		return new Texture2D( Device, size, format, isStaging: true );
	}

	public ISampler CreateSampler () {
		return new SamplerState( Device );
	}

	public IFramebuffer CreateFramebuffer ( IEnumerable<IDeviceTexture2D> attachments, IDeviceTexture2D? depthStencilAttachment = null ) {
		return new TargetView( attachments.Select( x => ((Texture2D)x).Texture ), ((Texture2D?)depthStencilAttachment)?.Texture );
	}

	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		return new Direct3D11ImmediateCommandBuffer( this, Context );
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

	Dictionary<BlendState, ID3D11BlendState> blendStates = new();
	public ID3D11BlendState GetBlendState ( BlendState description ) {
		if ( !description.IsEnabled )
			description = default;

		if ( !blendStates.TryGetValue( description, out var value ) ) {
			static Blend blend ( BlendFactor factor ) {
				return factor switch {
					BlendFactor.Zero => Blend.Zero,
					BlendFactor.One => Blend.One,
					BlendFactor.Fragment => Blend.SourceColor,
					BlendFactor.FragmentInverse => Blend.InverseSourceColor,
					BlendFactor.Destination => Blend.DestinationColor,
					BlendFactor.DestinationInverse => Blend.InverseDestinationColor,
					BlendFactor.FragmentAlpha => Blend.SourceAlpha,
					BlendFactor.FragmentAlphaInverse => Blend.InverseSourceAlpha,
					BlendFactor.DestinationAlpha => Blend.DestinationAlpha,
					BlendFactor.DestinationAlphaInverse => Blend.InverseDestinationAlpha,
					BlendFactor.Constant => Blend.BlendFactor,
					BlendFactor.ConstantInverse => Blend.InverseBlendFactor,
					BlendFactor.ConstantAlpha => Blend.BlendFactor,
					BlendFactor.ConstantAlphaInverse => Blend.InverseBlendFactor,
					BlendFactor.AlphaSaturate => Blend.SourceAlphaSaturate,
					BlendFactor.SecondFragment => Blend.Source1Color,
					BlendFactor.SecondFragmentInverse => Blend.InverseSource1Color,
					BlendFactor.SecondFragmentAlpha => Blend.Source1Alpha,
					BlendFactor.SecondFragmentAlphaInverse or _ => Blend.InverseSource1Alpha
				};
			}

			static BlendOperation op ( BlendFunction function ) {
				return function switch {
					BlendFunction.Add => BlendOperation.Add,
					BlendFunction.Max => BlendOperation.Max,
					BlendFunction.Min => BlendOperation.Min,
					BlendFunction.FragmentMinusDestination => BlendOperation.Subtract,
					BlendFunction.DestinationMinusFragment or _ => BlendOperation.ReverseSubtract
				};
			}

			blendStates.Add( description, value = Device.CreateBlendState( new() {
				RenderTarget = { e0 = {
					BlendEnable = description.IsEnabled,
					RenderTargetWriteMask = ColorWriteEnable.All,
					BlendOperation = op( description.ColorFunction ),
					BlendOperationAlpha = op( description.AlphaFunction ),
					SourceBlend = blend( description.FragmentColorFactor ),
					SourceBlendAlpha = blend( description.FragmentAlphaFactor ),
					DestinationBlend = blend( description.DestinationColorFactor ),
					DestinationBlendAlpha = blend( description.DestinationAlphaFactor )
				} }
			} ) );
		}

		return value;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, i) in depthStencilStates ) {
			i.Dispose();
		}

		RasterizerState.Dispose();
		foreach ( var (_, i) in blendStates ) {
			i.Dispose();
		}
		blendStates.Clear();

		Device.Dispose();
		Context.Dispose();
	}
}
