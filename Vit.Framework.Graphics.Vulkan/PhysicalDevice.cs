using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class PhysicalDevice {
	public VkPhysicalDevice Source;
	public VkPhysicalDeviceProperties Properties;
	public VkQueueFamilyProperties[] Queues;
	public IEnumerable<uint> QueueIndices => Enumerable.Range( 0, Queues.Length ).Select( x => (uint)x );
	public Dictionary<VkQueueFlags, uint[]> QueuesByCapabilities;

	public string[] Extensions;

	public PhysicalDevice ( VkPhysicalDevice source ) {
		Source = source;
		Vk.vkGetPhysicalDeviceProperties( source, out Properties );
		Queues = VulkanExtensions.Out<VkQueueFamilyProperties>.Enumerate( Source, Vk.vkGetPhysicalDeviceQueueFamilyProperties );
		QueuesByCapabilities = Enum.GetValues<VkQueueFlags>().ToDictionary(
			x => x,
			x => Queues.Where( y => y.queueFlags.HasFlag( x ) ).ToArray().Select( x => (uint)Array.IndexOf( Queues, x ) ).ToArray()
		);
		foreach ( var i in Enum.GetValues<VkQueueFlags>() ) {
			if ( QueuesByCapabilities[i].Length == 0 )
				QueuesByCapabilities.Remove( i );
		}
		Extensions = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( Source, (nint)0, Vk.vkEnumerateDeviceExtensionProperties )
			.Select( x => x.extensionName.GetString() ).ToArray();
	}

	public bool QueueSupportsSurface ( VkSurfaceKHR surface, uint index ) {
		Vk.vkGetPhysicalDeviceSurfaceSupportKHR( Source, index, surface, out var supported );
		return supported;
	}

	public VkDevice CreateLogicalDevice ( string[] extensions, uint[] queues ) {
		var unique = queues.Distinct();
		floatCollectionPtr priorities = new float[] { 1f };

		VkDeviceQueueCreateInfo[] queueInfos = unique.Select( x => new VkDeviceQueueCreateInfo {
			sType = VkStructureType.DeviceQueueCreateInfo,
			queueFamilyIndex = x,
			queueCount = 1,
			pQueuePriorities = priorities
		} ).ToArray();

		using var _ = VulkanExtensions.CreatePointerArray( extensions, out var extensionsPtr );
		var features = new VkPhysicalDeviceFeatures() {

		};
		using VkDeviceCreateInfo info = new() {
			sType = VkStructureType.DeviceCreateInfo,
			pQueueCreateInfos = queueInfos,
			queueCreateInfoCount = (uint)queueInfos.Length,
			pEnabledFeatures = features,
			enabledExtensionCount = (uint)extensions.Length,
			ppEnabledExtensionNames = extensionsPtr
		};

		VulkanExtensions.Validate( Vk.vkCreateDevice( Source, info, 0, out var device ) );
		foreach ( var i in queueInfos ) {
			i.Dispose();
		} 
		return device;
	}

	public SwapchainDetails GetSwapChainDetails ( VkSurfaceKHR surface ) {
		Vk.vkGetPhysicalDeviceSurfaceCapabilitiesKHR( Source, surface, out var capabilities );
		return new() {
			Capabilities = capabilities,
			Formats = VulkanExtensions.Out<VkSurfaceFormatKHR>.Enumerate( Source, surface, Vk.vkGetPhysicalDeviceSurfaceFormatsKHR ),
			PresentModes = VulkanExtensions.Out<VkPresentModeKHR>.Enumerate( Source, surface, Vk.vkGetPhysicalDeviceSurfacePresentModesKHR )
		};
	}

	public struct SwapchainDetails {
		public VkSurfaceCapabilitiesKHR Capabilities;
		public VkSurfaceFormatKHR[] Formats;
		public VkPresentModeKHR[] PresentModes;
	}
}
