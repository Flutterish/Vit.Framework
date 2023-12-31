﻿using System.Runtime.InteropServices;
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
	public readonly VulkanInstance VulkanInstance;

	public unsafe PhysicalDevice ( VulkanInstance vulkan, VkPhysicalDevice handle ) {
		VulkanInstance = vulkan;
		Instance = handle;

		Vk.vkGetPhysicalDeviceProperties( this, out var properties );
		Properties = properties;
		Vk.vkGetPhysicalDeviceFeatures( this, out Features );

		Name = InteropExtensions.GetString( properties.deviceName, 256 );

		QueueFamilies = VulkanExtensions.Out<VkQueueFamilyProperties>.Enumerate( Instance, Vk.vkGetPhysicalDeviceQueueFamilyProperties )
			.Select( (x, i) => new QueueFamily( x, (uint)i ) ).ToArray();

		Extensions = GetSupportedExtensions( (CString)0 );
	}

	public unsafe SwapchainInfo? GetSwapchainInfo ( VkSurfaceKHR surface ) {
		if ( SwapchainInfo.RequiredExtensions.Except( Extensions ).Any() )
			return null;

		var info = new SwapchainInfo();
		info.GraphicsFamily = QueueFamilies.FirstOrDefault()!;
		if ( info.GraphicsFamily == null )
			return null;

		foreach ( var i in QueueFamilies ) {
			Vk.vkGetPhysicalDeviceSurfaceSupportKHR( this, i.Index, surface, out var supported ).Validate();
			if ( supported ) {
				info.PresentFamily = i;
				break;
			}
		}
		if ( info.PresentFamily == null )
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

	public unsafe string[] GetSupportedExtensions ( CString layer ) {
		var props = VulkanExtensions.Out<VkExtensionProperties>.Enumerate( Instance, (byte*)layer, Vk.vkEnumerateDeviceExtensionProperties );
		return props.Select( VulkanExtensions.GetName ).ToArray();
	}

	public unsafe (string name, string description)[] GetSupportedLayers () {
		var props = VulkanExtensions.Out<VkLayerProperties>.Enumerate( Instance, Vk.vkEnumerateDeviceLayerProperties );
		return props.Select( x => (x.GetName(), x.GetDescription()) ).ToArray();
	}

	public uint FindMemoryType ( uint filter, VkMemoryPropertyFlags properties ) {
		Vk.vkGetPhysicalDeviceMemoryProperties( this, out var memoryProperties );

		var types = MemoryMarshal.CreateSpan( ref memoryProperties.memoryTypes_0, 32 );
		var heaps = MemoryMarshal.CreateSpan( ref memoryProperties.memoryHeaps_0, 16 );

		for ( int i = 0; i < memoryProperties.memoryTypeCount; i++ ) {
			if ( ( filter & ( 1 << i ) ) != 0 && ( types[i].propertyFlags & properties ) == properties ) {
				return (uint)i;
			}
		}

		throw new Exception( "idk no memory" );
	}

	public VkFormat GetBestSupportedFormat ( IEnumerable<VkFormat> candidates, VkFormatFeatureFlags features, VkImageTiling tiling = VkImageTiling.Optimal ) {
		foreach ( var i in candidates ) {
			Vk.vkGetPhysicalDeviceFormatProperties( this, i, out var props );
			if ( tiling == VkImageTiling.Optimal && ( props.optimalTilingFeatures & features ) == features )
				return i;
			if ( tiling == VkImageTiling.Linear && ( props.linearTilingFeatures & features ) == features )
				return i;
		}

		throw new Exception( "Could not find a suitable format" );
	}

	public VkFormatProperties GetFormatProperties ( VkFormat format ) {
		Vk.vkGetPhysicalDeviceFormatProperties( this, format, out var props );
		return props;
	}

	public IEnumerable<VkSampleCountFlags> GetSupportedColorDepthMultisampling () {
		var max = Properties.limits.framebufferColorSampleCounts & Properties.limits.framebufferDepthSampleCounts;
		return Enum.GetValues<VkSampleCountFlags>().Where( x => ( x & max ) != 0 );
	}
	public IEnumerable<VkSampleCountFlags> GetSupportedColorMultisampling () {
		var max = Properties.limits.framebufferColorSampleCounts;
		return Enum.GetValues<VkSampleCountFlags>().Where( x => ( x & max ) != 0 );
	}
	public VkSampleCountFlags GetMaxColorDepthMultisampling () {
		var max = Properties.limits.framebufferColorSampleCounts & Properties.limits.framebufferDepthSampleCounts;
		return Enum.GetValues<VkSampleCountFlags>().MaxBy( x => (int)(x & max) );
	}
}
