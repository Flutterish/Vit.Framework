using SDL2;
using Vit.Framework.Graphics.Direct3D11;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Windowing.Sdl;

class Direct3D11Window : SdlWindow {
	public Direct3D11Window ( SdlHost host ) : base( host, GraphicsApiType.Direct3D11 ) { }

	protected override void InitializeHints ( ref SDL.SDL_WindowFlags flags ) {
		
	}

	bool swapchainCreated;
	public override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain ( GraphicsApi api, SwapChainArgs args ) {
		if ( swapchainCreated )
			throw new NotImplementedException( "Surface recreation not implemented" );
		swapchainCreated = true;
		
		if ( api is not Direct3D11Api dx )
			throw new ArgumentException( "Graphics API must be an Direct3D11 API created from the same host as this window", nameof( api ) );

		SDL.SDL_SysWMinfo info = default;
		SDL.SDL_VERSION( out info.version );
		SDL.SDL_GetWindowWMInfo( Pointer, ref info );

		SwapChainDescription swapChainDescription = new() {
			BufferDescription = {
				RefreshRate = {
					Numerator = 0,
					Denominator = 1
				},
				Format = Format.B8G8R8A8_UNorm_SRgb
			},
			SampleDescription = {
				Count = (int)args.Multisample.Ideal,
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
			DeviceCreationFlags.Debug,
			new FeatureLevel[] { },
			swapChainDescription,
			out var swapchain,
			out var device,
			out var featureLevel,
			out var context
		) );

		var renderer = new Direct3D11Renderer( dx, device!, context! );

		return (new Graphics.Direct3D11.Queues.Swapchain( swapchain!, renderer, this, args ), renderer);
	}
}