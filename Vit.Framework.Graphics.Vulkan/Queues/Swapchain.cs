using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class Swapchain : DisposableVulkanObject<VkSwapchainKHR> {
	public readonly Device Device;
	public VkSurfaceKHR Surface { get; private set; }
	public readonly SwapchainFormat Format;
	public VkExtent2D Size { get; private set; }

	VkImage[] images = Array.Empty<VkImage>();
	VkImageView[] imageViews = Array.Empty<VkImageView>();
	FrameBuffer[] frames = Array.Empty<FrameBuffer>();

	VkSwapchainCreateInfoKHR createInfo;
	uint[]? queueIndices;
	public unsafe Swapchain ( Device device, VkSurfaceKHR surface, SwapchainFormat format, Size2<uint> pixelSize, IReadOnlyList<QueueFamily> queues ) {
		Device = device;
		Surface = surface;
		Format = format;
		Vk.vkGetPhysicalDeviceSurfaceCapabilitiesKHR( device.PhysicalDevice, surface, out var capabilities ).Validate();
		Size = getExtent( pixelSize, capabilities );

		VkSwapchainCreateInfoKHR info = createInfo = new() {
			sType = VkStructureType.SwapchainCreateInfoKHR,
			surface = surface,
			minImageCount = format.OptimalImageCount,
			imageFormat = format.Format.format,
			imageColorSpace = format.Format.colorSpace,
			imageExtent = Size,
			imageArrayLayers = 1,
			imageUsage = VkImageUsageFlags.ColorAttachment,
			preTransform = capabilities.currentTransform,
			compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR,
			presentMode = format.PresentMode,
			clipped = true
		};

		if ( queues.Count > 1 ) {
			queueIndices = queues.Select( x => x.Index ).ToArray();
			info.imageSharingMode = VkSharingMode.Concurrent;
			info.queueFamilyIndexCount = (uint)queueIndices.Length;
			info.pQueueFamilyIndices = queueIndices.Data();
		}
		else {
			info.imageSharingMode = VkSharingMode.Exclusive;
		}

		Vk.vkCreateSwapchainKHR( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		createImageViews();
	}

	public unsafe void Recreate ( Size2<uint> pixelSize ) {
		Vk.vkGetPhysicalDeviceSurfaceCapabilitiesKHR( Device.PhysicalDevice, Surface, out var capabilities ).Validate();
		var size = getExtent( pixelSize, capabilities );

		if ( size.width == 0 || size.height == 0 )
			return;

		Size = size;

		foreach ( var i in frames ) {
			i.Dispose();
		}

		var info = createInfo with {
			preTransform = capabilities.currentTransform,
			imageExtent = Size,
			oldSwapchain = Instance
		};

		Vk.vkCreateSwapchainKHR( Device, &info, VulkanExtensions.TODO_Allocator, out var newInstance ).Validate();
		Vk.vkDestroySwapchainKHR( Device, Instance, VulkanExtensions.TODO_Allocator ); // TODO give this to be destroyed after use (right now we wait for device to idle)
		Instance = newInstance;

		createImageViews();
		if ( renderPass != null ) {
			frames = Array.Empty<FrameBuffer>();
			SetRenderPass( renderPass );
		}
	}
	
	unsafe void createImageViews () {
		foreach ( var i in imageViews ) {
			Vk.vkDestroyImageView( Device, i, VulkanExtensions.TODO_Allocator );
		}

		images = VulkanExtensions.Out<VkImage>.Enumerate( Device.Handle, Instance, Vk.vkGetSwapchainImagesKHR );
		imageViews = new VkImageView[images.Length];

		VkImageViewCreateInfo viewInfo = new() {
			sType = VkStructureType.ImageViewCreateInfo,
			viewType = VkImageViewType.Image2D,
			format = Format.Format.format,
			subresourceRange = {
				aspectMask = VkImageAspectFlags.Color,
				baseMipLevel = 0,
				levelCount = 1,
				baseArrayLayer = 0,
				layerCount = 1
			}
		};
		for ( int i = 0; i < images.Length; i++ ) {
			viewInfo.image = images[i];
			Vk.vkCreateImageView( Device, &viewInfo, VulkanExtensions.TODO_Allocator, out imageViews[i] ).Validate();
		}
	}

	RenderPass renderPass = null!;
	Image depthAttachment = null!;
	Image msaaBuffer = null!;
	public unsafe void SetRenderPass ( RenderPass pass ) {
		renderPass = pass;
		depthAttachment ??= new( pass.Device );
		depthAttachment.Allocate( Size, VkImageUsageFlags.DepthStencilAttachment, 
			renderPass.AttachmentFormat, VkImageAspectFlags.Depth, samples: pass.Samples
		);
		msaaBuffer ??= new( pass.Device );
		msaaBuffer.Allocate( Size, VkImageUsageFlags.TransientAttachment | VkImageUsageFlags.ColorAttachment, 
			pass.ColorFormat, samples: pass.Samples
		);

		foreach ( var i in frames ) {
			i.Dispose();
		}

		frames = new FrameBuffer[imageViews.Length];
		for ( int i = 0; i < imageViews.Length; i++ ) {
			frames[i] = new( new[] { msaaBuffer.View, depthAttachment.View, imageViews[i] }, Size, pass );
		}
	}

	public VkResult GetNextFrame ( Semaphore imageAvailable, out FrameBuffer frame, out uint index ) {
		index = 0;
		var result = Vk.vkAcquireNextImageKHR( Device, this, ulong.MaxValue, imageAvailable, VkFence.Null, ref index );
		frame = frames[index];
		return result;
	}

	public unsafe VkResult Present ( VkQueue queue, uint index, VkSemaphore renderFinished ) {
		var swapChain = Instance;
		var info = new VkPresentInfoKHR() {
			sType = VkStructureType.PresentInfoKHR,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &renderFinished,
			swapchainCount = 1,
			pSwapchains = &swapChain,
			pImageIndices = &index
		};

		return Vk.vkQueuePresentKHR( queue, &info );
	}

	VkExtent2D getExtent ( Size2<uint> pixelSize, VkSurfaceCapabilitiesKHR capabilities ) {
		if ( capabilities.currentExtent.width != uint.MaxValue ) {
			return capabilities.currentExtent;
		}

		return new() {
			width = Math.Clamp( pixelSize.Width, capabilities.minImageExtent.width, capabilities.maxImageExtent.width ),
			height = Math.Clamp( pixelSize.Height, capabilities.minImageExtent.height, capabilities.maxImageExtent.height )
		};
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var i in frames ) {
			i.Dispose();
		}
		foreach ( var i in imageViews ) {
			Vk.vkDestroyImageView( Device, i, VulkanExtensions.TODO_Allocator );
		}
		depthAttachment?.Dispose();
		msaaBuffer?.Dispose();
		Vk.vkDestroySwapchainKHR( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
