using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanApi : GraphicsApi {
	public readonly VulkanInstance Instance;

	public VulkanApi ( VulkanInstance instance, IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType.Vulkan, capabilities ) {
		Instance = instance;
	}

	protected override void Dispose ( bool disposing ) {
		Instance.Dispose();
	}
}
