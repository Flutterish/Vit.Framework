using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Windowing;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Windowing;

public class Direct3D11WindowSurface : WindowGraphicsSurface {
	IDirect3D11Window window;
	public Direct3D11WindowSurface ( Direct3D11Api graphicsApi, WindowSurfaceArgs args, IDirect3D11Window window ) : base( graphicsApi, args ) {
		this.window = window;
	}

	protected override (ISwapchain swapchain, IRenderer renderer) CreateSwapchain () {
		var dx = (Direct3D11Api)GraphicsApi;

		SwapChainDescription swapChainDescription = new() {
			BufferDescription = {
				RefreshRate = {
					Numerator = 0,
					Denominator = 1
				},
				Format = Format.B8G8R8A8_UNorm_SRgb
			},
			SampleDescription = {
				Count = int.Max( 1, (int)Args.Multisample.Ideal ),
				Quality = 0
			},
			BufferUsage = Usage.RenderTargetOutput,
			BufferCount = 1,
			OutputWindow = window.GetWindowPointer(),
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

		return (new Queues.Swapchain( swapchain!, renderer, window, Args ), renderer);
	}
}
