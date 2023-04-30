using Vit.Framework.Graphics.Direct3D11.Rendering;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using Vit.Framework.Windowing;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Queues;

public class Swapchain : DisposableObject, ISwapchain {
	public readonly IDXGISwapChain Handle;
	public readonly Direct3D11Renderer Renderer;
	public readonly Window Window;

	TargetView backBuffer;
	public Swapchain ( IDXGISwapChain handle, Direct3D11Renderer renderer, Window window ) {
		Handle = handle;
		Renderer = renderer;
		Window = window;
		commandBuffer = new( renderer.Context );

		D3DExtensions.Validate( Handle.GetBuffer<ID3D11Texture2D>( 0, out var framebuffer ) );
		backBuffer = new( renderer.Device.CreateRenderTargetView( framebuffer ) );
		backBuffer.Size = window.PixelSize.Cast<uint>();
		framebuffer!.Release();
	}

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
		Handle.Dispose();
	}
}
