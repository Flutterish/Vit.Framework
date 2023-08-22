using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vit.Framework.Windowing;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Queues;

public class Swapchain : DisposableObject, ISwapchain {
	public readonly IDXGISwapChain Handle;
	public readonly Direct3D11Renderer Renderer;
	public readonly IWindow Window;

	ID3D11Texture2D framebuffer;
	ID3D11Texture2D depthStencil;

	TargetView backBuffer;
	public Swapchain ( IDXGISwapChain handle, Direct3D11Renderer renderer, IWindow window, WindowSurfaceArgs args ) {
		Handle = handle;
		Renderer = renderer;
		Window = window;
		commandBuffer = new( renderer, renderer.Context );

		BackbufferSize = window.PixelSize;
		D3DExtensions.Validate( Handle.GetBuffer<ID3D11Texture2D>( 0, out framebuffer! ) );
		depthStencil = renderer.Device.CreateTexture2D( new Texture2DDescription {
			Width = (int)BackbufferSize.Width,
			Height = (int)BackbufferSize.Height,
			MipLevels = 1,
			ArraySize = 1,
			SampleDescription = {
				Count = int.Max( 1, (int)args.Multisample.Ideal )
			},
			Format = Format.D24_UNorm_S8_UInt, // TODO use args for depth. idc for now
			BindFlags = BindFlags.DepthStencil
		} );
		backBuffer = new( new[] { framebuffer }, depthStencil );
		framebuffer!.Release();
	}

	public Size2<uint> BackbufferSize { get; }
	public void Recreate () {
		throw new NotImplementedException();
	}

	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		frameIndex = 0;
		return backBuffer;
	}

	public void Present ( int frameIndex ) {
		Handle.Present( 1, PresentFlags.None );
	}

	Direct3D11ImmediateCommandBuffer commandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBufferForPresentation () {
		return commandBuffer;
	}

	protected override void Dispose ( bool disposing ) {
		backBuffer.Dispose();
		depthStencil?.Dispose();
		framebuffer?.Dispose();
		Handle.Dispose();
	}
}
