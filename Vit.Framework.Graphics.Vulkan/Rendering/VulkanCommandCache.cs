using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanCommandCache : ICommandCache {
	public readonly CommandBuffer Buffer;

	public VulkanCommandCache ( CommandBuffer buffer ) {
		Buffer = buffer;
	}

	public DisposeAction<ICommandBuffer> RenderTo ( NativeFramebuffer framebuffer, Color4<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		VkClearColorValue color = clearColor is Color4<float> v
			? new VkClearColorValue( v.R, v.G, v.B, v.A )
			: new VkClearColorValue( 0, 0, 0, 1 );
		VkClearDepthStencilValue depthStencil = new VkClearDepthStencilValue( clearDepth ?? 0, clearStencil ?? 0 );
		Buffer.BeginRenderPass( (FrameBuffer)framebuffer, new VkClearValue { color = color }, new VkClearValue { depthStencil = depthStencil } );
		// TODO instead have separate clear commands

		return new DisposeAction<ICommandBuffer>( this, static self => {
			((VulkanCommandCache)self).Buffer.FinishRenderPass();
		} );
	}
}
