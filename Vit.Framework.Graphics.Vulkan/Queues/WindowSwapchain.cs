using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Memory;
using Vit.Framework.Windowing;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class WindowSwapchain : DisposableObject, ISwapchain {
	Swapchain swapchain;
	public readonly RenderPass ToScreenRenderPass;
	public readonly Queue PresentQueue;
	public readonly Window Window;
	public readonly VulkanRenderer Renderer;

	public WindowSwapchain ( Window window, Swapchain swapchain, Queue presentQueue, VulkanRenderer renderer, SwapChainArgs args ) {
		PresentQueue = presentQueue;
		var device = swapchain.Device;
		this.swapchain = swapchain;
		this.Window = window;
		this.Renderer = renderer;

		var formats = new AcceptableRange<(DepthFormat depth, StencilFormat stencil)> {
			Ideal = (args.Depth.Ideal, args.Stencil.Ideal),
			Minimum = (args.Depth.Minimum, args.Stencil.Minimum),
			Maximum = (args.Depth.Maximum, args.Stencil.Maximum),
			SearchMode = args.Depth.SearchMode
		}.Order( new[] {
			VkFormat.D16Unorm,
			VkFormat.D16UnormS8Uint,
			VkFormat.D24UnormS8Uint,
			VkFormat.D32Sfloat,
			VkFormat.D32SfloatS8Uint,
			VkFormat.S8Uint,
			VkFormat.Undefined
		}.Select( x => (x.GetDepthFormat(), x.GetStencilFormat()) ), ( value, goal ) => {
			return Math.Log2( ( (int)goal.depth - (int)value.depth ) + (double)( (int)goal.stencil - (int)value.stencil ) / 64 );
		} );

		var attachmentFormat = device.PhysicalDevice.GetBestSupportedFormat(
			formats.Select( x => x.GetFormat() ),
			VkFormatFeatureFlags.DepthStencilAttachment
		);
		var multisample = (VkSampleCountFlags)args.Multisample.Pick(
		( ( attachmentFormat.GetDepthFormat() != DepthFormat.None ) ? device.PhysicalDevice.GetSupportedColorDepthMultisampling() : device.PhysicalDevice.GetSupportedColorMultisampling() )
		.Select( x => (MultisampleFormat)x ),
			( value, goal ) => (int)goal - (int)value
		);
		ToScreenRenderPass = new RenderPass( device, multisample, swapchain.Format.Format.format, attachmentFormat );

		swapchain.SetRenderPass( ToScreenRenderPass );

		frameInfo = new() {
			ImageAvailable = device.CreateSemaphore(),
			RenderFinished = device.CreateSemaphore(),
			InFlight = device.CreateFence( signaled: true )
		};
	}

	FrameInfo frameInfo;
	protected struct FrameInfo : IDisposable {
		public required Semaphore ImageAvailable;
		public required Semaphore RenderFinished;
		public required Fence InFlight;

		public void Dispose () {
			ImageAvailable.Dispose();
			RenderFinished.Dispose();
			InFlight.Dispose();
		}
	};

	protected override void Dispose ( bool disposing ) {
		frameInfo.Dispose();
		swapchain.Dispose();
		ToScreenRenderPass.Dispose();
	}

	public void Recreate () {
		swapchain.Device.WaitIdle(); // TODO bad?
		swapchain.Recreate( Window.PixelSize );
	}

	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		frameInfo.InFlight.Wait();

		if ( !validateSwapchain( swapchain.GetNextFrame( frameInfo.ImageAvailable, out var fb, out var index ), recreateSuboptimal: false ) ) {
			frameIndex = -1;
			return null;
		}

		frameIndex = (int)index;
		return fb;
	}

	public void Present ( int frameIndex ) {
		validateSwapchain( swapchain.Present( PresentQueue, (uint)frameIndex, frameInfo.RenderFinished ), recreateSuboptimal: true );
	}

	public IImmediateCommandBuffer CreateImmediateCommandBufferForPresentation () {
		frameInfo.InFlight.Reset();

		Renderer.GraphicsCommands.Reset();
		Renderer.GraphicsCommands.Begin();
		return new VulkanImmediateCommandBuffer(
			Renderer.GraphicsCommands, Renderer,
			x => {
				x.Buffer.Finish();
				x.Buffer.Submit( Renderer.GraphicsQueue, frameInfo.ImageAvailable, frameInfo.RenderFinished, frameInfo.InFlight );
			}
		);
	}

	bool validateSwapchain ( VkResult result, bool recreateSuboptimal ) {
		if ( result >= 0 )
			return true;

		if ( ( result == VkResult.ErrorOutOfDateKHR || ( result == VkResult.SuboptimalKHR && recreateSuboptimal ) ) && !Window.IsClosed ) {
			Recreate();
		}

		return false;
	}
}
