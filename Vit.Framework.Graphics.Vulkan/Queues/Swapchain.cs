using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Windowing;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class Swapchain : DisposableObject, ISwapchain {
	public readonly VkSwapchainKHR Handle;
	public readonly SwapchainParams Params;
	public readonly Device Device;
	public readonly RenderPass RenderPass;
	Frame[] frames;
	Queue GraphicsQueue;
	Queue PresentQueue;

	IQueue ISwapchain.GraphicsQueue => GraphicsQueue;
	IQueue ISwapchain.PresentQueue => PresentQueue;

	public struct SwapchainParams {
		public PhysicalDevice Device;
		public PhysicalDevice.SwapchainDetails Swapchain;
		public VkSurfaceFormatKHR Format;
		public VkPresentModeKHR PresentMode;
		public uint OptimalImageCount;
	}

	struct Frame {
		public VkImage Image;
		public VkImageView View;
		public FrameBuffer Framebuffer;
	}

	unsafe Swapchain ( VkSwapchainKHR handle, SwapchainParams @params, Device device, VkExtent2D size, Queue graphics, Queue present ) {
		Handle = handle;
		Params = @params;
		Device = device;
		FrameBufferCount = (int)@params.OptimalImageCount;
		RenderPass = RenderPass.CreateForDrawingToWindow( device, @params );
		GraphicsQueue = graphics;
		PresentQueue = present;

		frames = VulkanExtensions.Out<VkImage>.Enumerate( device.Handle, Handle, Vk.vkGetSwapchainImagesKHR ).Select( image => {
			VkImageViewCreateInfo viewInfo = new() {
				sType = VkStructureType.ImageViewCreateInfo,
				image = image,
				viewType = VkImageViewType.Image2D,
				format = @params.Format.format,
				subresourceRange = new() {
					aspectMask = VkImageAspectFlags.Color,
					baseMipLevel = 0,
					levelCount = 1,
					baseArrayLayer = 0,
					layerCount = 1
				}
			};

			VulkanExtensions.Validate( Vk.vkCreateImageView( device, &viewInfo, VulkanExtensions.TODO_Allocator, out var view ) );

			VkFramebufferCreateInfo frameBufferinfo = new() {
				sType = VkStructureType.FramebufferCreateInfo,
				renderPass = RenderPass.Handle,
				attachmentCount = 1,
				pAttachments = &view,
				width = size.width,
				height = size.height,
				layers = 1
			};

			VulkanExtensions.Validate( Vk.vkCreateFramebuffer( device, &frameBufferinfo, VulkanExtensions.TODO_Allocator, out var fb ) );

			return new Frame {
				Image = image,
				View = view,
				Framebuffer = new FrameBuffer( Device, fb )
			};
		} ).ToArray();
	}

	public int FrameBufferCount { get; }
	public (IFrameBuffer frameBuffer, int index) GetNextFrameBuffer ( IGpuBarrier frameAvailableBarrier, ICpuBarrier? cpuBarrier ) {
		uint index = 0;
		VulkanExtensions.Validate( Vk.vkAcquireNextImageKHR( Device, Handle, ulong.MaxValue, frameAvailableBarrier.Semaphore(), cpuBarrier.Fence(), ref index ) );

		return (frames[index].Framebuffer, (int)index);
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var i in frames ) {
			i.Framebuffer.Dispose();
		}
		RenderPass.Dispose();
		Vk.vkDestroySwapchainKHR( Device, Handle, VulkanExtensions.TODO_Allocator );
	}

	static CString[] swapchainDeviceExtensions = { "VK_KHR_swapchain" };
	public static unsafe (Swapchain, Device) Create ( VulkanInstance vulkan, VkSurfaceKHR surface, Window window ) {
		var @params = vulkan.GetBestParamsForSurface( surface );

		var graphicsQueueIndex = @params.Device.QueuesByCapabilities[VkQueueFlags.Graphics][0];
		var presentQueueIndex = @params.Device.QueueIndices.First( i => @params.Device.QueueSupportsSurface( surface, i ) );

		var queues = new[] { graphicsQueueIndex, presentQueueIndex };
		var device = @params.Device.CreateLogicalDevice( swapchainDeviceExtensions, queues );
		Queue graphicsQueue = device.GetQueue( graphicsQueueIndex );
		Queue presentQueue = device.GetQueue( presentQueueIndex );

		var swapchainSize = getSwapSize( window, @params.Swapchain.Capabilities );
		var swapchainInfo = new VkSwapchainCreateInfoKHR() {
			sType = VkStructureType.SwapchainCreateInfoKHR,
			minImageCount = @params.OptimalImageCount,
			imageFormat = @params.Format.format,
			imageColorSpace = @params.Format.colorSpace,
			imageExtent = swapchainSize,
			imageArrayLayers = 1,
			imageUsage = VkImageUsageFlags.ColorAttachment,
			preTransform = @params.Swapchain.Capabilities.currentTransform,
			compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR,
			presentMode = @params.PresentMode,
			clipped = true,
			surface = surface
		};

		if ( graphicsQueue != presentQueue ) {
			swapchainInfo.imageSharingMode = VkSharingMode.Concurrent;
			swapchainInfo.queueFamilyIndexCount = 2;
			swapchainInfo.pQueueFamilyIndices = queues.Data();
		}
		else {
			swapchainInfo.imageSharingMode = VkSharingMode.Exclusive;
		}

		VulkanExtensions.Validate( Vk.vkCreateSwapchainKHR( device, &swapchainInfo, VulkanExtensions.TODO_Allocator, out var swapchain ) );

		return (new( swapchain, @params, device, swapchainSize, graphicsQueue, presentQueue ), device);
	}

	static VkExtent2D getSwapSize ( Window window, VkSurfaceCapabilitiesKHR capabilities ) {
		if ( capabilities.currentExtent.width != uint.MaxValue ) {
			return capabilities.currentExtent;
		}

		return new() {
			width = Math.Clamp( (uint)window.PixelWidth, capabilities.minImageExtent.width, capabilities.maxImageExtent.width ),
			height = Math.Clamp( (uint)window.PixelHeight, capabilities.minImageExtent.height, capabilities.maxImageExtent.height )
		};
	}
}
