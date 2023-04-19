using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanImmediateCommandBuffer : VulkanCommandCache, IImmediateCommandBuffer {
	Action<VulkanImmediateCommandBuffer> submitAction;
	public VulkanImmediateCommandBuffer ( CommandBuffer buffer, Action<VulkanImmediateCommandBuffer> submitAction ) : base( buffer ) {
		this.submitAction = submitAction;
	}

	public void Dispose () {
		submitAction( this );
	}
}
