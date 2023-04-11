using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class CommandBuffer : VulkanObject<VkCommandBuffer> {
	public unsafe CommandBuffer ( CommandPool pool ) {
		var info = new VkCommandBufferAllocateInfo() {
			sType = VkStructureType.CommandBufferAllocateInfo,
			commandPool = pool,
			level = VkCommandBufferLevel.Primary,
			commandBufferCount = 1
		};

		Vk.vkAllocateCommandBuffers( pool.Device, &info, out Instance ).Validate();
	}

	public void Reset () {
		Vk.vkResetCommandBuffer( this, VkCommandBufferResetFlags.None );
	}

	public unsafe void Begin ( VkCommandBufferUsageFlags flags = VkCommandBufferUsageFlags.None ) {
		var info = new VkCommandBufferBeginInfo() {
			sType = VkStructureType.CommandBufferBeginInfo,
			flags = flags
		};

		Vk.vkBeginCommandBuffer( this, &info ).Validate();
	}

	public unsafe void BeginRenderPass ( FrameBuffer framebuffer, VkClearValue? clear = null ) {
		VkClearValue clearValue = clear.GetValueOrDefault();
		var info = new VkRenderPassBeginInfo() {
			sType = VkStructureType.RenderPassBeginInfo,
			renderPass = framebuffer.RenderPass,
			framebuffer = framebuffer,
			renderArea = {
				extent = framebuffer.Size
			}
		};

		if ( clear.HasValue ) {
			info.clearValueCount = 1;
			info.pClearValues = &clearValue;
		}

		Vk.vkCmdBeginRenderPass( this, &info, VkSubpassContents.Inline );
	}

	public void BindPipeline ( Pipeline pipeline ) {
		Vk.vkCmdBindPipeline( this, VkPipelineBindPoint.Graphics, pipeline );
	}

	public unsafe void BindDescriptor ( VkPipelineLayout layout, VkDescriptorSet descriptor ) {
		Vk.vkCmdBindDescriptorSets( this, VkPipelineBindPoint.Graphics, layout, 0, 1, &descriptor, 0, 0 );
	}

	public unsafe void BindVertexBuffer<T> ( Buffer<T> buffer ) where T : unmanaged {
		VkBuffer vkbuffer = buffer.Handle;
		ulong offset = 0;
		Vk.vkCmdBindVertexBuffers( this, 0, 1, &vkbuffer, &offset );
	}

	void bindIndexBuffer<T> ( Buffer<T> buffer, VkIndexType indexType ) where T : unmanaged {
		Vk.vkCmdBindIndexBuffer( this, buffer, 0, indexType );
	}
	public unsafe void BindIndexBuffer ( Buffer<ushort> buffer ) {
		bindIndexBuffer( buffer, VkIndexType.Uint16 );
	}
	public unsafe void BindIndexBuffer ( Buffer<uint> buffer ) {
		bindIndexBuffer( buffer, VkIndexType.Uint32 );
	}

	public unsafe void SetViewPort ( VkViewport viewport ) {
		Vk.vkCmdSetViewport( this, 0, 1, &viewport );
	}

	public unsafe void SetScissor ( VkRect2D scissor ) {
		Vk.vkCmdSetScissor( this, 0, 1, &scissor );
	}

	public void Draw ( uint vertexCount, uint instanceCount = 1 ) {
		Vk.vkCmdDraw( this, vertexCount, instanceCount, 0, 0 );
	}

	public void DrawIndexed ( uint indexCount, uint instanceCount = 1 ) {
		Vk.vkCmdDrawIndexed( this, indexCount, instanceCount, 0, 0, 0 );
	}

	public unsafe void Copy ( VkBuffer source, VkBuffer destination, ulong size, ulong srcOffset = 0, ulong dstOffset = 0 ) {
		var region = new VkBufferCopy() {
			size = size,
			srcOffset = srcOffset,
			dstOffset = dstOffset
		};

		Vk.vkCmdCopyBuffer( this, source, destination, 1, &region );
	}

	public unsafe void Copy ( VkBuffer source, VkImage destination, VkExtent3D size ) {
		var region = new VkBufferImageCopy() {
			bufferOffset = 0,
			bufferRowLength = 0,
			bufferImageHeight = 0,
			imageSubresource = {
				aspectMask = VkImageAspectFlags.Color,
				mipLevel = 0,
				baseArrayLayer = 0,
				layerCount = 1
			},
			imageOffset = { x = 0, y = 0, z = 0 },
			imageExtent = size
		};

		Vk.vkCmdCopyBufferToImage( this, source, destination, VkImageLayout.TransferDstOptimal, 1, &region );
	}

	public void FinishRenderPass () {
		Vk.vkCmdEndRenderPass( this );
	}

	public void Finish () {
		Vk.vkEndCommandBuffer( this ).Validate();
	}

	public unsafe void Submit ( VkQueue queue, VkSemaphore waitUntil = default, VkSemaphore signal = default, VkFence inFlight = default ) {
		VkPipelineStageFlags flags = VkPipelineStageFlags.ColorAttachmentOutput;
		VkCommandBuffer buffer = this;
		var info = new VkSubmitInfo() {
			sType = VkStructureType.SubmitInfo,
			waitSemaphoreCount = waitUntil == default ? 0u : 1,
			pWaitSemaphores = &waitUntil,
			pWaitDstStageMask = &flags,
			commandBufferCount = 1,
			pCommandBuffers = &buffer,
			signalSemaphoreCount = signal == default ? 0u : 1,
			pSignalSemaphores = &signal
		};

		Vk.vkQueueSubmit( queue, 1, &info, inFlight ).Validate();
	}
}
