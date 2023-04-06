using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class PhysicalDevice : VulkanObject<VkPhysicalDevice> {
	public readonly string Name;
	public VkPhysicalDeviceType DeviceType => Properties.deviceType;
	public readonly VkPhysicalDeviceProperties Properties;
	public readonly VkPhysicalDeviceFeatures Features;
	public readonly IReadOnlyList<QueueFamily> QueueFamilies;

	public readonly IReadOnlyList<string> Extensions;

	public unsafe PhysicalDevice ( VkPhysicalDevice handle ) {
		Instance = handle;

		Vk.vkGetPhysicalDeviceProperties( this, out var properties );
		Properties = properties;
		Vk.vkGetPhysicalDeviceFeatures( this, out Features );

		Name = InteropExtensions.GetString( properties.deviceName, 256 );

		QueueFamilies = VulkanExtensions.Out<VkQueueFamilyProperties>.Enumerate( Instance, Vk.vkGetPhysicalDeviceQueueFamilyProperties )
			.Select( (x, i) => new QueueFamily( x, (uint)i ) ).ToArray();

		Extensions = GetSupportedExtensions();
	}

	public unsafe SwapchainInfo? GetSwapchainInfo ( VkSurfaceKHR surface ) {
		if ( SwapchainInfo.RequiredExtensions.Except( Extensions ).Any() )
			return null;

		var info = new SwapchainInfo();
		info.GraphicsQueue = QueueFamilies.FirstOrDefault()!;
		if ( info.GraphicsQueue == null )
			return null;

		foreach ( var i in QueueFamilies ) {
			Vk.vkGetPhysicalDeviceSurfaceSupportKHR( this, i.Index, surface, out var supported ).Validate();
			if ( supported ) {
				info.PresentQueue = i;
				break;
			}
		}
		if ( info.PresentQueue == null )
			return null;

		Vk.vkGetPhysicalDeviceSurfaceCapabilitiesKHR( this, surface.Handle, out info.Capabilities ).Validate();
		info.Formats = VulkanExtensions.Out<VkSurfaceFormatKHR>.Enumerate( Instance, surface, Vk.vkGetPhysicalDeviceSurfaceFormatsKHR );
		if ( info.Formats.Length == 0 )
			return null;

		info.PresentModes = VulkanExtensions.Out<VkPresentModeKHR>.Enumerate( Instance, surface, Vk.vkGetPhysicalDeviceSurfacePresentModesKHR );
		if ( info.PresentModes.Length == 0 )
			return null;

		return info;
	}
	
	public Device CreateDevice ( IReadOnlyList<CString> extensions, IReadOnlyList<CString> layers, IEnumerable<QueueFamily> queues ) {
		return new Device( this, extensions, layers, queues );
	}

	public unsafe string[] GetSupportedExtensions ( string? layer = null ) {
		var props = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( Instance, (byte*)(CString)layer, Vk.vkEnumerateDeviceExtensionProperties );
		return props.Select( VulkanExtensions.GetName ).ToArray();
	}

	public unsafe (string name, string description)[] GetSupportedLayers () {
		var props = VulkanExtensions.Out<VkLayerProperties>.Enumerate( Instance, Vk.vkEnumerateDeviceLayerProperties );
		return props.Select( x => (x.GetName(), x.GetDescription()) ).ToArray();
	}
}
