using System.Globalization;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Queues;

public class CommandBuffer : ICommandBuffer {
	public readonly VkCommandBuffer Handle;
	public readonly Queue Queue;

	public CommandBuffer ( Queue queue, VkCommandBuffer handle ) {
		Handle = handle;
		Queue = queue;
	}

	public void Reset () {
		VulkanExtensions.Validate( Vk.vkResetCommandBuffer( Handle, 0 ) );
	}

	public unsafe void BeginRecodring () {
		VkCommandBufferBeginInfo info = new() {
			sType = VkStructureType.CommandBufferBeginInfo
		};

		VulkanExtensions.Validate( Vk.vkBeginCommandBuffer( Handle, &info ) );

		//VkClearValue clearValue = default;
		//VkRenderPassBeginInfo beginInfo = new() {
		//	sType = VkStructureType.RenderPassBeginInfo,
		//	renderPass = renderPass,
		//	framebuffer = frames[imageIndex].Framebuffer,
		//	renderArea = {
		//		offset = { x = 0, y = 0 },
		//		extent = swapchainSize
		//	},
		//	clearValueCount = 1,
		//	pClearValues = &clearValue
		//};
		//Vk.vkCmdBeginRenderPass( Handle, &beginInfo, VkSubpassContents.Inline );
	}

	public void FinishRecodring () {
		Vk.vkCmdEndRenderPass( Handle );
		VulkanExtensions.Validate( Vk.vkEndCommandBuffer( Handle ) );
	}

	public unsafe void Submit ( IGpuBarrier beginBarrier, IGpuBarrier finishedBarrier, ICpuBarrier renderingFinishedBarrier ) {
		var beginSemaphore = beginBarrier.Semaphore();
		var endSemaphore = finishedBarrier.Semaphore();
		var handle = Handle;

		VkPipelineStageFlags flags = VkPipelineStageFlags.ColorAttachmentOutput;
		VkSubmitInfo submitInfo = new() {
			sType = VkStructureType.SubmitInfo,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &beginSemaphore,
			pWaitDstStageMask = &flags,
			commandBufferCount = 1,
			pCommandBuffers = &handle,
			signalSemaphoreCount = 1,
			pSignalSemaphores = &endSemaphore
		};

		VulkanExtensions.Validate( Vk.vkQueueSubmit( Queue.Handle, 1, &submitInfo, renderingFinishedBarrier.Fence() ) );
	}

	public void DrawPrimitives ( uint count ) {
		Vk.vkCmdDraw( Handle, count, 1, 0, 0 );
	}
}
