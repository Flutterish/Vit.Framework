using SDL2;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Interop;
using Vit.Framework.Threading;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Windowing.Sdl;

public class SdlDirect3D11RenderThread : AppThread {
	SdlWindow window;
	public SdlDirect3D11RenderThread ( SdlWindow window, string name ) : base( name ) {
		this.window = window;
	}

	ID3D11DeviceContext context = null!;
	ID3D11RenderTargetView targetView = null!;
	ID3D11InputLayout inputLayout = null!;
	ID3D11Buffer buffer = null!;
	ID3D11VertexShader vs = null!;
	ID3D11PixelShader fs = null!;
	IDXGISwapChain swapChain = null!;
	protected override void Initialize () {
		SDL.SDL_SysWMinfo info = default;
		SDL.SDL_VERSION( out info.version );
		SDL.SDL_GetWindowWMInfo( window.Pointer, ref info );

		SwapChainDescription swapChainDescription = new() {
			BufferDescription = {
				RefreshRate = {
					Numerator = 0,
					Denominator = 1
				},
				Format = Format.B8G8R8A8_UNorm_SRgb
			},
			SampleDescription = {
				Count = 1,
				Quality = 0
			},
			BufferUsage = Usage.RenderTargetOutput,
			BufferCount = 1,
			OutputWindow = info.info.win.window,
			Windowed = true
		};

		D3DExtensions.Validate( D3D11.D3D11CreateDeviceAndSwapChain( 
			null, 
			DriverType.Hardware,
			DeviceCreationFlags.Singlethreaded | DeviceCreationFlags.Debug,
			new FeatureLevel[] {},
			swapChainDescription,
			out swapChain,
			out var device,
			out var featureLevel,
			out context!
		) );

		D3DExtensions.Validate( swapChain!.GetBuffer<ID3D11Texture2D>( 0, out var framebuffer ) );
		targetView = device!.CreateRenderTargetView( framebuffer );
		framebuffer!.Release();

		var shader = @"/* vertex attributes go here to input to the vertex shader */
		struct vs_in {
			float3 position_local : POS;			
			float3 color : COL;
		};

		/* outputs from vertex shader go here. can be interpolated to pixel shader */
		struct vs_out {
			float4 position_clip : SV_POSITION; // required output of VS
			float3 color : COL;
		};

		vs_out vs_main(vs_in input) {
			vs_out output = (vs_out)0; // zero the memory first
			output.position_clip = float4(input.position_local, 1.0);
			output.color = input.color;
			return output;
		}

		float4 ps_main(vs_out input) : SV_TARGET {
			return float4( input.color.r, input.color.g, input.color.b, 1.0 ); // must return an RGBA colour
		}";

		var vsData = Vortice.D3DCompiler.Compiler.Compile( shader, "vs_main", "", "vs_5_0" );
		var fsData = Vortice.D3DCompiler.Compiler.Compile( shader, "ps_main", "", "ps_5_0" );

		vs = device.CreateVertexShader( vsData.Span );
		fs = device.CreatePixelShader( fsData.Span );

		inputLayout = device.CreateInputLayout( new InputElementDescription[] {
			new() {
				SemanticName = "POS", SemanticIndex = 0, Format = Format.R32G32B32_Float,
				Slot = 0, AlignedByteOffset = 0, Classification = InputClassification.PerVertexData,
				InstanceDataStepRate = 0
			},
			new() {
				SemanticName = "COL", SemanticIndex = 0, Format = Format.R32G32B32_Float,
				Slot = 0, AlignedByteOffset = 3 * sizeof(float), Classification = InputClassification.PerVertexData,
				InstanceDataStepRate = 0
			}
		}, vsData.Span );

		float[] vertices = {
		   0.0f,  0.5f,  0.0f, 1, 0, 0, // point at top
		   0.5f, -0.5f,  0.0f, 0, 1, 0, // point at bottom-right
		  -0.5f, -0.5f,  0.0f, 0, 0, 1  // point at bottom-left
		};

		D3DExtensions.Validate( device.CreateBuffer( new BufferDescription {
			ByteWidth = vertices.Length * sizeof(float),
			Usage = ResourceUsage.Default,
			BindFlags = BindFlags.VertexBuffer
		},  new SubresourceData {
			DataPointer = vertices.DataPtr()
		}, out buffer ) );

		var rng = new Random();
		clearColor = new( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 );
	}

	Vortice.Mathematics.Color4 clearColor;
	protected override void Loop () {
		context.ClearRenderTargetView( targetView, clearColor );
		context.RSSetViewport( 0, 0, window.Width, window.Height );
		context.OMSetRenderTargets( targetView );

		context.IASetPrimitiveTopology( PrimitiveTopology.TriangleList );
		context.IASetInputLayout( inputLayout );
		context.IASetVertexBuffer( 0, buffer, 6 * sizeof(float) );

		context.VSSetShader( vs );
		context.PSSetShader( fs );

		context.Draw( 3, 0 );

		swapChain.Present( 1, PresentFlags.None );
		Sleep(1);
	}

	protected override void Dispose ( bool disposing ) {
		context.Dispose();
		targetView.Dispose();
		inputLayout.Dispose();
		buffer.Dispose();
		vs.Dispose();
		fs.Dispose();
		swapChain.Dispose();
	}
}
