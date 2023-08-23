using System.Diagnostics;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class Image : DisposableVulkanObject<VkImage>, IVulkanTexture, IDeviceTexture2D {
	public VulkanTextureType Type => VulkanTextureType.Image;
	public readonly Device Device;
	public VkExtent2D Size;
	VkDeviceMemory memory;
	bool allocated = false;

	public unsafe Image ( Device device ) {
		Device = device;
	}

	Size2<uint> ITexture2D.Size => new( Size.width, Size.height );
	public PixelFormat Format { get; }
	public unsafe Image ( Device device, Size2<uint> size, PixelFormat format ) {
		Debug.Assert( format == PixelFormat.Rgba8 );
		Format = format;

		Device = device;
		Allocate(
			new VkExtent2D { width = size.Width, height = size.Height },
			VkImageUsageFlags.TransferDst | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.Sampled,
			VkFormat.R8g8b8a8Unorm
		);
	}

	public ITexture2DView CreateView () {
		return new ImageView( this, VkFormat.R8g8b8a8Unorm, VkImageAspectFlags.Color, 1 );
	}

	VkImageLayout layout;
	public uint MipMapLevels { get; private set; } = 0;
	public VkSampleCountFlags Samples { get; private set; } = VkSampleCountFlags.None;
	public unsafe void Allocate ( 
		VkExtent2D size, VkImageUsageFlags usage, VkFormat format, 
		VkImageAspectFlags aspect = VkImageAspectFlags.Color, VkImageTiling tiling = VkImageTiling.Optimal, 
		bool prepareForMipMaps = false, VkSampleCountFlags samples = VkSampleCountFlags.Count1 ) 
	{
		if ( allocated ) {
			Free();
		}
		allocated = true;

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
		Vk.vkDestroyImage( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkFreeMemory( Device, memory, VulkanExtensions.TODO_Allocator );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		if ( Instance == VkImage.Null )
			return;

		Free();
	}
}
