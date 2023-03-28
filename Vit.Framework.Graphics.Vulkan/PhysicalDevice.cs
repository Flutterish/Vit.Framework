using System.Buffers;
using System.Runtime.InteropServices;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public unsafe class PhysicalDevice {
	VulkanInstance instance;
	public VkPhysicalDevice Source;
	public VkPhysicalDeviceProperties Properties;
	public VkQueueFamilyProperties[] Queues;
	public IEnumerable<uint> QueueIndices => Enumerable.Range( 0, Queues.Length ).Select( x => (uint)x );
	public Dictionary<VkQueueFlags, uint[]> QueuesByCapabilities;

	public string[] Extensions;

	public PhysicalDevice ( VulkanInstance instance, VkPhysicalDevice source ) {
		this.instance = instance;
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
		var props = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( Source, (byte*)0, Vk.vkEnumerateDeviceExtensionProperties ).ToArray();
		Extensions = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( Source, (byte*)0, Vk.vkEnumerateDeviceExtensionProperties )
			.Select( extensionNameToString ).ToArray();
	}

	string extensionNameToString ( VkExtensionProperties props ) {
		return Marshal.PtrToStringUTF8( (nint)(&props) )!;
	}

	public bool QueueSupportsSurface ( VkSurfaceKHR surface, uint index ) {
		Vk.vkGetPhysicalDeviceSurfaceSupportKHR( Source, index, surface, out var supported );
		return supported;
	}

	public Device CreateLogicalDevice ( CString[] extensions, uint[] queues ) {
		var unique = queues.Distinct();
		var priorities = 1f;
		var prioritiesPtr = &priorities;

		VkDeviceQueueCreateInfo[] queueInfos = unique.Select( x => new VkDeviceQueueCreateInfo {
			sType = VkStructureType.DeviceQueueCreateInfo,
			queueFamilyIndex = x,
			queueCount = 1,
			pQueuePriorities = prioritiesPtr
		} ).ToArray();

		var extensionPtrs = extensions.MakeArray();
		var features = new VkPhysicalDeviceFeatures() {

		};
		VkDeviceCreateInfo info = new() {
			sType = VkStructureType.DeviceCreateInfo,
			pQueueCreateInfos = queueInfos.Data(),
			queueCreateInfoCount = (uint)queueInfos.Length,
			pEnabledFeatures = &features,
			enabledExtensionCount = (uint)extensions.Length,
			ppEnabledExtensionNames = extensionPtrs.Data()
		};

		VulkanExtensions.Validate( Vk.vkCreateDevice( Source, &info, instance.Allocator, out var device ) );
		return new( device );
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
