using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanRenderer : Renderer {
	public readonly VulkanInstance Instance;

	public VulkanRenderer ( VulkanInstance instance, IEnumerable<RenderingCapabilities> capabilities ) : base( RenderingApi.Vulkan, capabilities ) {
		Instance = instance;
	}

	protected override void Dispose ( bool disposing ) {
		Instance.Dispose();
	}
}
