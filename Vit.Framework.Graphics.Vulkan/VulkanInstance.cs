using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanInstance : DisposableVulkanObject<VkInstance> {
	public unsafe VulkanInstance ( IReadOnlyList<CString> extensions, IReadOnlyList<CString> layers ) {
		VkApplicationInfo appInfo = new() {
			sType = VkStructureType.ApplicationInfo,
			applicationVersion = new Version( 1, 0, 0 ),
			engineVersion = new Version( 1, 0, 0 ),
			apiVersion = new Version( 1, 0, 0 )
		};

		var extensionNames = extensions.MakeArray();
		var layerNames = layers.MakeArray();
		VkInstanceCreateInfo info = new() {
			sType = VkStructureType.InstanceCreateInfo,
			pApplicationInfo = &appInfo,
			enabledExtensionCount = (uint)extensions.Count,
			ppEnabledExtensionNames = extensionNames.Data(),
			enabledLayerCount = (uint)layers.Count,
			ppEnabledLayerNames = layerNames.Data()
		};

		Vk.vkCreateInstance( &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();

		if ( extensions.Any( x => x.ToString() == "VK_EXT_debug_utils" ) ) {
			// TODO setup debug calls
		}
	}

	PhysicalDevice[]? physicalDevices;
	public unsafe IReadOnlyList<PhysicalDevice> PhysicalDevices {
		get {
			if ( physicalDevices == null ) {
				var handles = VulkanExtensions.Out<VkPhysicalDevice>.Enumerate( Instance, Vk.vkEnumeratePhysicalDevices );
				physicalDevices = handles.Select( x => new PhysicalDevice( x ) ).ToArray();
			}

			return physicalDevices;
		}
	}

	public (PhysicalDevice device, SwapchainInfo swapchainInfo) GetBestDeviceInfo ( VkSurfaceKHR surface ) {
		return PhysicalDevices.Select( x => (
			device: x,
			swapchain: x.GetSwapchainInfo( surface )!
		) ).Where(
			x => x.swapchain != null
		).OrderBy( x => x.device.DeviceType switch {
			VkPhysicalDeviceType.DiscreteGpu => 1,
			_ => 2
		} ).ThenByDescending(
			x => x.device.Properties.limits.maxImageDimension2D
		).First();
	}

	public static unsafe string[] GetSupportedExtensions ( string? layer = null ) {
		var props = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( (byte*)(CString)layer, Vk.vkEnumerateInstanceExtensionProperties );
		return props.Select( VulkanExtensions.GetName ).ToArray();
	}

	public static unsafe (string name, string description)[] GetSupportedLayers () {
		var props = VulkanExtensions.Out<VkLayerProperties>.Enumerate( Vk.vkEnumerateInstanceLayerProperties );
		return props.Select( x => (x.GetName(), x.GetDescription()) ).ToArray();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyInstance( this, VulkanExtensions.TODO_Allocator );
	}
}
