using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Graphics.Vulkan.Windowing;
using Vit.Framework.Input;
using Vit.Framework.Interop;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vulkan;
using Semaphore = Vit.Framework.Graphics.Vulkan.Synchronisation.Semaphore;

namespace Vit.Framework.Tests;

public abstract class VulkanRenderThread : AppThread {
	protected DateTime StartTime { get; private set; }
	protected DateTime LastFrameTime { get; private set; }

	protected readonly Host Host;
	protected readonly Window Window;
	protected readonly VulkanRenderer Renderer;
	protected readonly VulkanInstance Vulkan;

	protected readonly BinaryStateTracker<Key> Keys = new();
	public VulkanRenderThread ( Window window, Host host, string name ) : base( name ) {
		LastFrameTime = StartTime = DateTime.Now;
		Host = host;
		Window = window;

		Renderer = (VulkanRenderer)host.CreateRenderer( RenderingApi.Vulkan, new[] { RenderingCapabilities.DrawToWindow }, new() {
			Window = window
		} )!;
		Vulkan = Renderer.Instance;

		Window.PhysicalKeyDown += Keys.Add;
		Window.PhysicalKeyUp += Keys.Remove;

		window.Resized += onWindowResized;
	}

	bool windowResized;
	void onWindowResized ( Window _ ) {
		windowResized = true;
	}

	protected SwapchainInfo SwapchainInfo { get; private set; } = null!;
	protected Device Device { get; private set; } = null!;

	protected VkQueue GraphicsQueue { get; private set; }
	protected VkQueue PresentQueue { get; private set; }

	protected Swapchain Swapchain { get; private set; } = null!;
	protected RenderPass ToScreenRenderPass { get; private set; } = null!;

	protected CommandPool GraphicsCommandPool { get; private set; } = null!;
	protected CommandPool CopyCommandPool { get; private set; } = null!;

	protected override void Initialize () {
		var surface = ( (IVulkanWindow)Window ).GetSurface( Vulkan );
		var (physicalDevice, swapchainInfo) = Vulkan.GetBestDeviceInfo( surface );
		SwapchainInfo = swapchainInfo;

		Device = physicalDevice.CreateDevice( SwapchainInfo.RequiredExtensionsCstr, new CString[] { }, new[] {
			swapchainInfo.GraphicsQueue,
			swapchainInfo.PresentQueue
		} );

		GraphicsQueue = Device.GetQueue( swapchainInfo.GraphicsQueue );
		PresentQueue = Device.GetQueue( swapchainInfo.PresentQueue );
		Swapchain = Device.CreateSwapchain( surface, swapchainInfo.SelectBest(), Window.PixelSize );

		var depthFormat = Device.PhysicalDevice.GetBestSupportedFormat(
			new[] { VkFormat.D32Sfloat, VkFormat.D32SfloatS8Uint, VkFormat.D24UnormS8Uint },
			VkFormatFeatureFlags.DepthStencilAttachment
		);
		ToScreenRenderPass = new RenderPass( Device, Device.PhysicalDevice.GetMaxColorDepthMultisampling(), Swapchain.Format.Format.format, depthFormat );

		Swapchain.SetRenderPass( ToScreenRenderPass );

		GraphicsCommandPool = Device.CreateCommandPool( SwapchainInfo.GraphicsQueue );
		CopyCommandPool = Device.CreateCommandPool( SwapchainInfo.GraphicsQueue, VkCommandPoolCreateFlags.ResetCommandBuffer | VkCommandPoolCreateFlags.Transient );

		frameInfo = new() {
			ImageAvailable = Device.CreateSemaphore(),
			RenderFinished = Device.CreateSemaphore(),
			InFlight = Device.CreateFence( signaled: true ),
			Commands = GraphicsCommandPool.CreateCommandBuffer()
		};
	}

	protected TimeSpan DeltaTime { get; private set; }
	protected TimeSpan TimeSinceStartup { get; private set; }

	FrameInfo frameInfo;
	protected struct FrameInfo : IDisposable {
		public required Semaphore ImageAvailable;
		public required Semaphore RenderFinished;
		public required Fence InFlight;
		public required CommandBuffer Commands;

		public void Dispose () {
			ImageAvailable.Dispose();
			RenderFinished.Dispose();
			InFlight.Dispose();
		}
	};

	protected sealed override void Loop () {
		var now = DateTime.Now;
		DeltaTime = now - LastFrameTime;
		TimeSinceStartup = now - StartTime;
		LastFrameTime = now;

		if ( windowResized ) {
			windowResized = false;
			recreateSwapchain();
		}

		var info = frameInfo;
		var commands = info.Commands;

		info.InFlight.Wait();

		if ( !validateSwapchain( Swapchain.GetNextFrame( info.ImageAvailable, out var frame, out var index ), recreateSuboptimal: false ) ) {
			return;
		}

		info.InFlight.Reset();

		Render( frameInfo, frame );

		commands.Submit( GraphicsQueue, info.ImageAvailable, info.RenderFinished, info.InFlight );
		validateSwapchain( Swapchain.Present( PresentQueue, index, info.RenderFinished ), recreateSuboptimal: true );
	}

	protected abstract void Render ( FrameInfo info, FrameBuffer frame );

	bool validateSwapchain ( VkResult result, bool recreateSuboptimal ) {
		if ( result >= 0 )
			return true;

		if ( ( result == VkResult.ErrorOutOfDateKHR || ( result == VkResult.SuboptimalKHR && recreateSuboptimal ) ) && !Window.IsClosed ) {
			recreateSwapchain();
		}

		return false;
	}

	void recreateSwapchain () {
		Device.WaitIdle(); // TODO bad
		Swapchain.Recreate( Window.PixelSize );
	}

	protected sealed override void Dispose ( bool disposing ) {
		Window.Resized -= onWindowResized;
		if ( !IsInitialized )
			return;

		Device.WaitIdle();

		Dispose();

		frameInfo.Dispose();

		GraphicsCommandPool.Dispose();
		CopyCommandPool.Dispose();

		ToScreenRenderPass.Dispose();
		Swapchain.Dispose();
		Device.Dispose();
		Renderer.Dispose();
	}

	new protected virtual void Dispose () { }
}
