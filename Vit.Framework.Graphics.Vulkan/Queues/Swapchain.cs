using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class Swapchain : DisposableVulkanObject<VkSwapchainKHR> {
	public readonly Device Device;
	public readonly VkSurfaceKHR Surface;
	public readonly SwapchainFormat Format;
	public readonly VkExtent2D Size;

	public readonly VkImage[] Images;
	public readonly VkImageView[] ImageViews;
	FrameBuffer[] frames = Array.Empty<FrameBuffer>();
	public unsafe Swapchain ( Device device, VkSurfaceKHR surface, SwapchainFormat format, VkExtent2D size, IReadOnlyList<QueueFamily> queues ) {
		Device = device;
		Surface = surface;
		Format = format;
		Size = size;

		VkSwapchainCreateInfoKHR info = new() {
			sType = VkStructureType.SwapchainCreateInfoKHR,
			surface = surface,
			minImageCount = format.OptimalImageCount,
			imageFormat = format.Format.format,
			imageColorSpace = format.Format.colorSpace,
			imageExtent = size,
			imageArrayLayers = 1,
			imageUsage = VkImageUsageFlags.ColorAttachment,
			preTransform = format.Capabilities.currentTransform,
			compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR,
			presentMode = format.PresentMode,
			clipped = true
		};

		if ( queues.Count > 1 ) {
			var indices = queues.Select( x => x.Index ).ToArray();
			info.imageSharingMode = VkSharingMode.Concurrent;
			info.queueFamilyIndexCount = (uint)indices.Length;
			info.pQueueFamilyIndices = indices.Data();
		}
		else {
			info.imageSharingMode = VkSharingMode.Exclusive;
		}

		Vk.vkCreateSwapchainKHR( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		(Images, ImageViews) = createImageViews();
	}
	
	unsafe (VkImage[], VkImageView[]) createImageViews () {
		var images = VulkanExtensions.Out<VkImage>.Enumerate( Device.Handle, Instance, Vk.vkGetSwapchainImagesKHR );
		var views = new VkImageView[images.Length];
		var frameBuffers = new VkFramebuffer[images.Length];

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
			Vk.vkCreateImageView( Device, &viewInfo, VulkanExtensions.TODO_Allocator, out views[i] ).Validate();
		}

		return (images, views);
	}

	RenderPass renderPass = null!;
	public unsafe void SetRenderPass ( RenderPass pass ) {
		renderPass = pass;
		foreach ( var i in frames ) {
			i.Dispose();
		}

		frames = new FrameBuffer[ImageViews.Length];
		for ( int i = 0; i < ImageViews.Length; i++ ) {
			frames[i] = new( ImageViews[i], Size, pass );
		}
	}

	public bool GetNextFrame ( Semaphore imageAvailable, [NotNullWhen(true)] out FrameBuffer? frame, out uint index ) {
		index = 0;
		var result = Vk.vkAcquireNextImageKHR( Device, this, ulong.MaxValue, imageAvailable, VkFence.Null, ref index );
		frame = frames[index];
		return result >= 0;
	}

	public unsafe void Present ( VkQueue queue, uint index, VkSemaphore renderFinished ) {
		var swapChain = Instance;
		var info = new VkPresentInfoKHR() {
			sType = VkStructureType.PresentInfoKHR,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &renderFinished,
			swapchainCount = 1,
			pSwapchains = &swapChain,
			pImageIndices = &index
		};

		Vk.vkQueuePresentKHR( queue, &info );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var i in frames ) {
			i.Dispose();
		}
		foreach ( var i in ImageViews ) {
			Vk.vkDestroyImageView( Device, i, VulkanExtensions.TODO_Allocator );
		}
		Vk.vkDestroySwapchainKHR( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
