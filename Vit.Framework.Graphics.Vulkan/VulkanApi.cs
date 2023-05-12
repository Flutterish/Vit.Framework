using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.Vulkan,
		Name = "Vulkan",
		Version = -1
	};

	public readonly VulkanInstance Instance;

	public VulkanApi ( VulkanInstance instance, IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
		Instance = instance;
	}

	protected override void Dispose ( bool disposing ) {
		Instance.Dispose();
	}
}
