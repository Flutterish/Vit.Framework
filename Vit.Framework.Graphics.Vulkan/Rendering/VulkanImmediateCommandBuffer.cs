using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanImmediateCommandBuffer : VulkanDeferredCommandBuffer, IImmediateCommandBuffer {
	Action<VulkanImmediateCommandBuffer> submitAction;
	public VulkanImmediateCommandBuffer ( CommandBuffer buffer, VulkanRenderer renderer, Action<VulkanImmediateCommandBuffer> submitAction ) : base( buffer, renderer ) {
		this.submitAction = submitAction;
	}

	public void Dispose () {
		submitAction( this );
	}
}
