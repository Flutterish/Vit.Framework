using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanRenderer : Renderer {
	public readonly Device Device;
	public readonly Queue GraphicsQueue;
	public readonly CommandPool GraphicsCommandPool;
	public readonly CommandPool CopyCommandPool;
	public readonly CommandBuffer GraphicsCommands;

	public VulkanRenderer ( VulkanApi api, Device device, Queue graphicsQueue ) : base( api ) {
		Device = device;
		GraphicsQueue = graphicsQueue;
		GraphicsCommandPool = Device.CreateCommandPool( graphicsQueue );
		CopyCommandPool = Device.CreateCommandPool( graphicsQueue, VkCommandPoolCreateFlags.ResetCommandBuffer | VkCommandPoolCreateFlags.Transient );

		GraphicsCommands = GraphicsCommandPool.CreateCommandBuffer();
	}

	public override void WaitIdle () {
		Device.WaitIdle();
	}

	public override Matrix4<T> CreateLeftHandCorrectionMatrix<T> () {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}

	protected override void Dispose ( bool disposing ) {
		Device.Dispose();
	}
}
