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

	public unsafe void Begin () {
		var info = new VkCommandBufferBeginInfo() {
			sType = VkStructureType.CommandBufferBeginInfo
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

	public void Bind ( Pipeline pipeline ) {
		Vk.vkCmdBindPipeline( this, VkPipelineBindPoint.Graphics, pipeline );
	}

	public unsafe void Bind<T> ( VertexBuffer<T> buffer ) where T : unmanaged {
		VkBuffer vkbuffer = buffer.Handle;
		ulong offset = 0;
		Vk.vkCmdBindVertexBuffers( this, 0, 1, &vkbuffer, &offset );
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

	public void FinishRenderPass () {
		Vk.vkCmdEndRenderPass( this );
	}

	public void Finish () {
		Vk.vkEndCommandBuffer( this ).Validate();
	}

	public unsafe void Submit ( VkQueue queue, VkSemaphore imageAvailable, VkSemaphore renderFinished, VkFence inFlight ) {
		VkPipelineStageFlags flags = VkPipelineStageFlags.ColorAttachmentOutput;
		VkCommandBuffer buffer = this;
		var info = new VkSubmitInfo() {
			sType = VkStructureType.SubmitInfo,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &imageAvailable,
			pWaitDstStageMask = &flags,
			commandBufferCount = 1,
			pCommandBuffers = &buffer,
			signalSemaphoreCount = 1,
			pSignalSemaphores = &renderFinished
		};

		Vk.vkQueueSubmit( queue, 1, &info, inFlight ).Validate();
	}
}
