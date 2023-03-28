using Vit.Framework.Interop;
using Vulkan;
using static Vit.Framework.Graphics.Vulkan.Queues.Swapchain;

namespace Vit.Framework.Graphics.Vulkan;

public unsafe class VulkanInstance : IDisposable {
	public readonly VkInstance Instance;
	public VkAllocationCallbacks* Allocator => (VkAllocationCallbacks*)0;

	public string[] Extensions;
	public string[] Layers;

	public static string[] GetAvailableExtensions () {
		return VulkanExtensions.Out<VkExtensionProperties>.Enumerate( (byte*)0, Vk.vkEnumerateInstanceExtensionProperties )
			.Select( x => InteropExtensions.GetString( x.extensionName ) ).ToArray();
	}

	public static string[] GetAvailableLayers () {
		return VulkanExtensions.Out<VkLayerProperties>.Enumerate( Vk.vkEnumerateInstanceLayerProperties )
			.Select( x => InteropExtensions.GetString( x.layerName ) ).ToArray();
	}

	public unsafe VulkanInstance ( IReadOnlyList<CString> extensions, IReadOnlyList<CString> layers ) {
		Extensions = extensions.Select( x => x.ToString() ).ToArray();
		Layers = layers.Select( x => x.ToString() ).ToArray();

		VkApplicationInfo appinfo = new() {
			sType = VkStructureType.ApplicationInfo,
			apiVersion = new Version( 1, 3, 0 ),
			applicationVersion = new Version( 1, 2, 0 ),
			engineVersion = new Version( 1, 2, 0 )
		};

		var extensionPtrs = extensions.MakeArray();
		var layerPtrs = layers.MakeArray();
		VkInstanceCreateInfo createInfo = new() {
			sType = VkStructureType.InstanceCreateInfo,
			pApplicationInfo = &appinfo,
			enabledExtensionCount = (uint)extensions.Count,
			ppEnabledExtensionNames = extensionPtrs.Data(),
			enabledLayerCount = (uint)layers.Count,
			ppEnabledLayerNames = layerPtrs.Data()
		};

		VulkanExtensions.Validate( Vk.vkCreateInstance( &createInfo, Allocator, out Instance ) );
	}

	public PhysicalDevice[] GetAllPhysicalDevices () {
		return VulkanExtensions.Out<VkPhysicalDevice>.Enumerate( Instance, Vk.vkEnumeratePhysicalDevices ).Select( x => new PhysicalDevice( this, x ) ).ToArray();
	}

	static string[] requiredDeviceExtensions = { "VK_KHR_swapchain" };
	public SwapchainParams GetBestParamsForSurface ( VkSurfaceKHR surface ) {
		var (physicalDevice, swapchain) = GetAllPhysicalDevices().Where( x => {
			return !requiredDeviceExtensions.Except( x.Extensions ).Any()
				&& x.QueuesByCapabilities.ContainsKey( VkQueueFlags.Graphics )
				&& x.QueueIndices.Any( i => x.QueueSupportsSurface( surface, i ) );
		} ).Select( x => (device: x, swapchain: x.GetSwapChainDetails( surface )) ).Where( x => {
			return x.swapchain.Formats.Any() && x.swapchain.PresentModes.Any();
		} ).OrderBy( x => x.device.Properties.deviceType switch {
			VkPhysicalDeviceType.DiscreteGpu => 1,
			VkPhysicalDeviceType.IntegratedGpu => 2,
			_ => 3
		} ).First();

		var format = swapchain.Formats.OrderBy( x => x switch { { format: VkFormat.B8g8r8a8Srgb, colorSpace: VkColorSpaceKHR.SrgbNonlinearKHR } => 1,
			_ => 2
		} ).First();
		var presentMode = swapchain.PresentModes.OrderBy( x => x switch {
			VkPresentModeKHR.MailboxKHR => 1,
			VkPresentModeKHR.FifoKHR => 9,
			VkPresentModeKHR.FifoRelaxedKHR => 10,
			VkPresentModeKHR.ImmediateKHR or _ => 11
		} ).First();

		uint imageCount = Math.Min(
			swapchain.Capabilities.minImageCount + 1,
			swapchain.Capabilities.maxImageCount == 0 ? uint.MaxValue : swapchain.Capabilities.maxImageCount
		);

		return new() {
			Device = physicalDevice,
			Swapchain = swapchain,
			Format = format,
			PresentMode = presentMode,
			OptimalImageCount = imageCount
		};
	}

	private bool isDisposed;
	protected virtual void Dispose ( bool disposing ) {
		Vk.vkDestroyInstance( Instance, Allocator );
	}

	~VulkanInstance () {
	    Dispose( disposing: false );
	}

	public void Dispose () {
		if ( isDisposed || Instance.Handle == 0 )
			return;

		Dispose( disposing: true );
		isDisposed = true;
		GC.SuppressFinalize( this );
	}
}
