using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vulkan;
using Buffer = Vit.Framework.Graphics.Vulkan.Buffers.Buffer;

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

	public unsafe void BeginRenderPass ( FrameBuffer framebuffer, params VkClearValue[] clear ) {
		var info = new VkRenderPassBeginInfo() {
			sType = VkStructureType.RenderPassBeginInfo,
			renderPass = framebuffer.RenderPass,
			framebuffer = framebuffer,
			renderArea = {
				extent = framebuffer.Size
			},
			clearValueCount = (uint)clear.Length,
			pClearValues = clear.Data()
		};

		Vk.vkCmdBeginRenderPass( this, &info, VkSubpassContents.Inline );
	}

	public void BindPipeline ( Pipeline pipeline ) {
		Vk.vkCmdBindPipeline( this, VkPipelineBindPoint.Graphics, pipeline );
	}

	public unsafe void BindDescriptors ( VkPipelineLayout layout, ReadOnlySpan<VkDescriptorSet> descriptors ) {
		Vk.vkCmdBindDescriptorSets( this, VkPipelineBindPoint.Graphics, layout, 0, (uint)descriptors.Length, descriptors.Data(), 0, 0 );
	}

	public unsafe void BindVertexBuffer ( IVulkanHandle<VkBuffer> buffer ) {
		VkBuffer vkbuffer = buffer.Handle;
		ulong offset = 0;
		Vk.vkCmdBindVertexBuffers( this, 0, 1, &vkbuffer, &offset );
	}

	public unsafe void BindVertexBuffers ( ReadOnlySpan<VkBuffer> buffers, ReadOnlySpan<ulong> offsets ) {
		Vk.vkCmdBindVertexBuffers( this, 0, (uint)buffers.Length, buffers.Data(), offsets.Data() );
	}

	void bindIndexBuffer ( IVulkanHandle<VkBuffer> buffer, VkIndexType indexType, ulong offset ) {
		Vk.vkCmdBindIndexBuffer( this, buffer.Handle, offset, indexType );
	}
	public unsafe void BindU16IndexBuffer ( Buffer buffer, ulong offset ) {
		bindIndexBuffer( buffer, VkIndexType.Uint16, offset );
	}
	public unsafe void BindU32IndexBuffer ( Buffer buffer, ulong offset ) {
		bindIndexBuffer( buffer, VkIndexType.Uint32, offset );
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

	public void DrawIndexed ( uint indexCount, uint offset ) {
		Vk.vkCmdDrawIndexed( this, indexCount, 1, offset, 0, 0 );
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
