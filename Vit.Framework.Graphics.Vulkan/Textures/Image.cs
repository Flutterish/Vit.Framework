using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
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
	public uint MipMapLevels { get; private set; } = 0;
	public VkSampleCountFlags Samples { get; private set; } = VkSampleCountFlags.None;
	public unsafe void Allocate ( Image<Rgba32> source, CommandBuffer commands ) {
		var format = VkFormat.R8g8b8a8Srgb;
		var aspect = VkImageAspectFlags.Color;
		var extent = new VkExtent2D( source.Width, source.Height );
		Allocate(
			extent, 
			VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.Sampled,
			format,
			prepareForMipMaps: true
		);

		var length = (uint)source.Width * (uint)source.Height;
		buffer.Allocate( length );

		TransitionLayout( VkImageLayout.TransferDstOptimal, aspect, commands );

		source.CopyPixelDataTo( buffer.GetDataSpan( source.Width * source.Height ) );
		buffer.Unmap();
		commands.Copy( buffer, Handle, new VkExtent3D {
			width = extent.width,
			height = extent.height,
			depth = 1
		} );

		GenerateMipMaps( commands, aspect );
	}
	public unsafe void Transfer<TPixel> ( ReadOnlySpan<TPixel> data, CommandBuffer commands ) where TPixel : unmanaged {
		var length = Size.width * Size.height;
		buffer.Allocate( length );

		TransitionLayout( VkImageLayout.TransferDstOptimal, VkImageAspectFlags.Color, commands ); // TODO also bad! (aspect)

		data.CopyTo( MemoryMarshal.Cast<Rgba32, TPixel>( buffer.GetDataSpan( (int)length ) ) ); // TODO bad!
		buffer.Unmap();
		commands.Copy( buffer, Handle, new VkExtent3D {
			width = Size.width,
			height = Size.height,
			depth = 1
		} );

		GenerateMipMaps( commands, VkImageAspectFlags.Color );
	}
	public unsafe void Allocate ( 
		VkExtent2D size, VkImageUsageFlags usage, VkFormat format, 
		VkImageAspectFlags aspect = VkImageAspectFlags.Color, VkImageTiling tiling = VkImageTiling.Optimal, 
		bool prepareForMipMaps = false, VkSampleCountFlags samples = VkSampleCountFlags.Count1 ) 
	{
		if ( view != VkImageView.Null ) {
			Free();
		}

		layout = VkImageLayout.Undefined;
		Size = size;
		Samples = samples;

		MipMapLevels = prepareForMipMaps ? (uint)Math.Floor( Math.Log2( Math.Max( size.width, size.height ) ) ) + 1 : 1;
		var info = new VkImageCreateInfo() {
			sType = VkStructureType.ImageCreateInfo,
			imageType = VkImageType.Image2D,
			extent = {
				width = Size.width,
				height = Size.height,
				depth = 1
			},
			mipLevels = MipMapLevels,
			arrayLayers = 1,
			format = format,
			tiling = tiling,
			initialLayout = VkImageLayout.Undefined,
			usage = usage,
			sharingMode = VkSharingMode.Exclusive,
			samples = samples
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
				levelCount = MipMapLevels,
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
				levelCount = MipMapLevels
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

	public unsafe void GenerateMipMaps ( CommandBuffer commands, VkImageAspectFlags aspect ) {
		var barrier = new VkImageMemoryBarrier() {
			sType = VkStructureType.ImageMemoryBarrier,
			image = this,
			srcQueueFamilyIndex = ~0u,
			dstQueueFamilyIndex = ~0u,
			subresourceRange = {
				aspectMask = aspect,
				baseArrayLayer = 0,
				levelCount = 1,
				layerCount = 1
			},
			oldLayout = VkImageLayout.TransferDstOptimal,
			newLayout = VkImageLayout.TransferSrcOptimal,
			srcAccessMask = VkAccessFlags.TransferWrite,
			dstAccessMask = VkAccessFlags.TransferRead
		};

		var finalBarrier = barrier with {
			oldLayout = VkImageLayout.TransferSrcOptimal,
			newLayout = VkImageLayout.ShaderReadOnlyOptimal,
			srcAccessMask = VkAccessFlags.TransferRead,
			dstAccessMask = VkAccessFlags.ShaderRead
		};

		int width = (int)Size.width;
		int height = (int)Size.height;
		for ( uint i = 1; i < MipMapLevels; i++ ) {
			barrier.subresourceRange.baseMipLevel = i - 1;
			finalBarrier.subresourceRange.baseMipLevel = i - 1;

			Vk.vkCmdPipelineBarrier( 
				commands,
				VkPipelineStageFlags.Transfer, VkPipelineStageFlags.Transfer, 0,
				0, 0,
				0, 0,
				1, &barrier
			);

			var blit = new VkImageBlit() {
				srcOffsets_0 = { x = 0, y = 0, z = 0 },
				srcOffsets_1 = { x = width, y = height, z = 1 },
				srcSubresource = {
					aspectMask = aspect,
					mipLevel = i - 1,
					baseArrayLayer = 0,
					layerCount = 1
				},
				dstOffsets_0 = { x = 0, y = 0, z = 0 },
				dstOffsets_1 = {
					x = width > 1 ? width / 2 : 1,
					y = height > 1 ? height / 2 : 1,
					z = 1
				},
				dstSubresource = {
					aspectMask = aspect,
					mipLevel = i,
					baseArrayLayer = 0,
					layerCount = 1
				}
			};

			Vk.vkCmdBlitImage(
				commands,
				this, VkImageLayout.TransferSrcOptimal,
				this, VkImageLayout.TransferDstOptimal,
				1, &blit,
				VkFilter.Linear
			);

			Vk.vkCmdPipelineBarrier(
				commands,
				VkPipelineStageFlags.Transfer, VkPipelineStageFlags.FragmentShader, 0,
				0, 0,
				0, 0,
				1, &finalBarrier
			);

			if ( width > 1 ) width /= 2;
			if ( height > 1 ) height /= 2;
		}

		finalBarrier = finalBarrier with {
			oldLayout = VkImageLayout.TransferDstOptimal,
			srcAccessMask = VkAccessFlags.TransferWrite
		};
		finalBarrier.subresourceRange.baseMipLevel = MipMapLevels - 1;
		Vk.vkCmdPipelineBarrier(
			commands,
			VkPipelineStageFlags.Transfer, VkPipelineStageFlags.FragmentShader, 0,
			0, 0,
			0, 0,
			1, &finalBarrier
		);
	}

	public unsafe void Free () {
		Vk.vkDestroyImageView( Device, view, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyImage( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkFreeMemory( Device, memory, VulkanExtensions.TODO_Allocator );
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
