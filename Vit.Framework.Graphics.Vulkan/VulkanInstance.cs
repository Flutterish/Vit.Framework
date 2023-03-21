using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public unsafe class VulkanInstance : IDisposable {
	public readonly VkInstance Instance;
	public VkAllocationCallbacks* Allocator => (VkAllocationCallbacks*)0;

	public delegate bool ExtensionSelector ( string[] available, out CString[] selected );
	public unsafe VulkanInstance ( ExtensionSelector extensionSelector, ExtensionSelector layerSelector ) {
		VkApplicationInfo appinfo = new() {
			sType = VkStructureType.ApplicationInfo,
			apiVersion = new Version( 1, 3, 0 ),
			applicationVersion = new Version( 1, 2, 0 ),
			engineVersion = new Version( 1, 2, 0 )
		};

		var availableExtensions = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( (byte*)0, Vk.vkEnumerateInstanceExtensionProperties )
			.Select( x => VulkanExtensions.GetString( x.extensionName ) ).ToArray();
		if ( !extensionSelector( availableExtensions, out var extensions ) )
			throw new InvalidOperationException( "Available vulkan extensions are not acceptable" );

		var availableLayers = VulkanExtensions.Out<VkLayerProperties>.Enumerate( Vk.vkEnumerateInstanceLayerProperties )
			.Select( x => VulkanExtensions.GetString( x.layerName ) ).ToArray();
		if ( !layerSelector( availableLayers, out var layers ) )
			throw new InvalidOperationException( "Available vulkan layers are not acceptable" );

		var extensionPtrs = extensions.MakeArray();
		var layerPtrs = layers.MakeArray();
		VkInstanceCreateInfo createInfo = new() {
			sType = VkStructureType.InstanceCreateInfo,
			pApplicationInfo = &appinfo,
			enabledExtensionCount = (uint)extensions.Length,
			ppEnabledExtensionNames = extensionPtrs.Data(),
			enabledLayerCount = (uint)layers.Length,
			ppEnabledLayerNames = layerPtrs.Data()
		};

		VulkanExtensions.Validate( Vk.vkCreateInstance( &createInfo, Allocator, out Instance ) );
		//Vk.LoadInstanceFunctionPointers( Instance );
	}

	public PhysicalDevice[] GetAllPhysicalDevices () {
		return VulkanExtensions.Out<VkPhysicalDevice>.Enumerate( Instance, Vk.vkEnumeratePhysicalDevices ).Select( x => new PhysicalDevice( this, x ) ).ToArray();
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
