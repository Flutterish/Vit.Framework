using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class Image : DisposableVulkanObject<VkImage>, IVulkanHandle<VkImageView> {
	public readonly Device Device;
	HostBuffer<Rgba32> buffer;
	public VkExtent2D Size;
	VkDeviceMemory memory;
	VkImageView view;
	public VkImageView View => view;
	VkImageView IVulkanHandle<VkImageView>.Handle => view;

	public unsafe Image ( Device device ) {
		Device = device;
		buffer = new( device, VkBufferUsageFlags.TransferSrc );
	}

	CommandBuffer Allocate ( Image<Rgba32> source, CommandPool pool ) {
		var commands = pool.CreateCommandBuffer();
		commands.Begin( VkCommandBufferUsageFlags.OneTimeSubmit );
		Allocate( source, commands );
		commands.Finish();

		return commands;
	}
	public void AllocateAndTransfer ( Image<Rgba32> source, CommandPool pool, VkQueue queue ) {
		var copy = Allocate( source, pool );
		copy.Submit( queue );
		Vk.vkQueueWaitIdle( queue );
		pool.FreeCommandBuffer( copy );
		FreeStagingBuffer();
	}

	VkImageLayout layout;
	public unsafe void Allocate ( Image<Rgba32> source, CommandBuffer commands ) {
		var format = VkFormat.R8g8b8a8Srgb;
		var aspect = VkImageAspectFlags.Color;
		var extent = new VkExtent2D( source.Width, source.Height );
		Allocate(
			extent, 
			VkImageUsageFlags.TransferDst | VkImageUsageFlags.Sampled,
			format
		);

		var length = (uint)source.Width * (uint)source.Height;
		buffer.Allocate( length );

		TransitionLayout( VkImageLayout.TransferDstOptimal, aspect, commands );
		commands.Copy( buffer, Handle, new VkExtent3D {
			width = extent.width,
			height = extent.height,
			depth = 1
		} );
		TransitionLayout( VkImageLayout.ShaderReadOnlyOptimal, aspect, commands );

		source.CopyPixelDataTo( buffer.GetDataSpan( source.Width * source.Height ) );
		buffer.Unmap();
	}
	public unsafe void Allocate ( VkExtent2D size, VkImageUsageFlags usage, VkFormat format, VkImageAspectFlags aspect = VkImageAspectFlags.Color, VkImageTiling tiling = VkImageTiling.Optimal ) {
		if ( view != VkImageView.Null ) {
			Free();
		}

		layout = VkImageLayout.Undefined;
		Size = size;

		var info = new VkImageCreateInfo() {
			sType = VkStructureType.ImageCreateInfo,
			imageType = VkImageType.Image2D,
			extent = {
				width = Size.width,
				height = Size.height,
				depth = 1
			},
			mipLevels = 1,
			arrayLayers = 1,
			format = format,
			tiling = tiling,
			initialLayout = VkImageLayout.Undefined,
			usage = usage,
			sharingMode = VkSharingMode.Exclusive,
			samples = VkSampleCountFlags.Count1
		};

		Vk.vkCreateImage( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		Vk.vkGetImageMemoryRequirements( Device, Instance, out var requirement );

		var allocInfo = new VkMemoryAllocateInfo() {
			sType = VkStructureType.MemoryAllocateInfo,
			allocationSize = requirement.size,
			memoryTypeIndex = Device.PhysicalDevice.FindMemoryType( requirement.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal )
		};
		Vk.vkAllocateMemory( Device, &allocInfo, VulkanExtensions.TODO_Allocator, out memory );
		Vk.vkBindImageMemory( Device, Instance, memory, 0 );

		var viewInfo = new VkImageViewCreateInfo() {
			sType = VkStructureType.ImageViewCreateInfo,
			image = this,
			viewType = VkImageViewType.Image2D,
			format = format,
			subresourceRange = {
				aspectMask = aspect,
				baseMipLevel = 0,
				levelCount = 1,
				baseArrayLayer = 0,
				layerCount = 1
			}
		};

		Vk.vkCreateImageView( Device, &viewInfo, VulkanExtensions.TODO_Allocator, out view ).Validate();
	}

	public unsafe void TransitionLayout ( VkImageLayout newLayout, VkImageAspectFlags aspect, CommandBuffer commands ) {
		var oldLayout = layout;
		layout = newLayout;
		VkPipelineStageFlags sourceStage;
		VkPipelineStageFlags destinationStage;
		var barrier = new VkImageMemoryBarrier() {
			sType = VkStructureType.ImageMemoryBarrier,
			oldLayout = oldLayout,
			newLayout = newLayout,
			srcQueueFamilyIndex = ~0u,
			dstQueueFamilyIndex = ~0u,
			image = this,
			subresourceRange = {
				aspectMask = aspect,
				baseMipLevel = 0,
				layerCount = 1,
				baseArrayLayer = 0,
				levelCount = 1
			}
		};
		if ( (oldLayout, newLayout) is (VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal ) ) {
			barrier.srcAccessMask = 0;
			barrier.dstAccessMask = VkAccessFlags.TransferWrite;
			sourceStage = VkPipelineStageFlags.TopOfPipe;
			destinationStage = VkPipelineStageFlags.Transfer;
		}
		else if ( (oldLayout, newLayout) is (VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal ) ) {
			barrier.srcAccessMask = VkAccessFlags.TransferWrite;
			barrier.dstAccessMask = VkAccessFlags.ShaderRead;
			sourceStage = VkPipelineStageFlags.Transfer;
			destinationStage = VkPipelineStageFlags.FragmentShader;
		}
		else {
			throw new ArgumentException( "Inavlid layouts" );
		}

		Vk.vkCmdPipelineBarrier( 
			commands, 
			sourceStage, destinationStage, 
			0, 
			0, 0, 
			0, 0, 
			1, &barrier 
		);
	}

	public unsafe void Free () {
		Vk.vkDestroyImageView( Device, view, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyImage( Device, Instance, VulkanExtensions.TODO_Allocator );
		FreeStagingBuffer();
	}

	public void FreeStagingBuffer () {
		buffer.Dispose();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		if ( Instance == VkImage.Null )
			return;

		Free();
	}
}
