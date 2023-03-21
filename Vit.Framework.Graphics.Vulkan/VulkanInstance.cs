using Vulkan;
using Version = global::Vulkan.Version;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanInstance : IDisposable {
	public readonly VkInstance Instance;

	public delegate bool ExtensionSelector ( string[] available, out string[] selected );
	public VulkanInstance ( ExtensionSelector extensionSelector, ExtensionSelector layerSelector ) {
		using VkApplicationInfo appinfo = new( new Version( 1, 2, 0 ), new Version( 1, 2, 0 ), new Version( 1, 3, 0 ) );

		var availableExtensions = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( (nint)0, Vk.vkEnumerateInstanceExtensionProperties )
			.Select( x => x.extensionName.GetString() ).ToArray();
		if ( !extensionSelector( availableExtensions, out var extensions ) )
			throw new InvalidOperationException( "Available vulkan extensions are not acceptable" );

		var availableLayers = VulkanExtensions.Out<VkLayerProperties>.Enumerate( Vk.vkEnumerateInstanceLayerProperties )
			.Select( x => x.layerName.GetString() ).ToArray();
		if ( !layerSelector( availableLayers, out var layers ) )
			throw new InvalidOperationException( "Available vulkan layers are not acceptable" );

		using var _ = VulkanExtensions.CreatePointerArray( extensions, out var extensionsPtr );
		using var __ = VulkanExtensions.CreatePointerArray( layers, out var layersPtr );
		using VkInstanceCreateInfo createInfo = new() {
			sType = VkStructureType.InstanceCreateInfo,
			pApplicationInfo = appinfo,
			enabledExtensionCount = (uint)extensions.Length,
			ppEnabledExtensionNames = extensionsPtr,
			enabledLayerCount = (uint)layers.Length,
			ppEnabledLayerNames = layersPtr
		};

		VulkanExtensions.Validate( Vk.vkCreateInstance( createInfo, IntPtr.Zero, out Instance ) );
		Vk.LoadInstanceFunctionPointers( Instance );
	}

	public PhysicalDevice[] GetAllPhysicalDevices () {
		return VulkanExtensions.Out<VkPhysicalDevice>.Enumerate( Instance, Vk.vkEnumeratePhysicalDevices ).Select( x => new PhysicalDevice( x ) ).ToArray();
	}

	private bool isDisposed;
	protected virtual void Dispose ( bool disposing ) {
		Vk.vkDestroyInstance( Instance, 0 );
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
