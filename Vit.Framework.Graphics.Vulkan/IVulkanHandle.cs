namespace Vit.Framework.Graphics.Vulkan;

public interface IVulkanHandle<T> where T : unmanaged {
	T Handle { get; }
}
