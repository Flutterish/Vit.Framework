using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Synchronisation;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class Device : DisposableVulkanObject<VkDevice> {
	public readonly IReadOnlyList<QueueFamily> QueueFamilies;
	public readonly PhysicalDevice PhysicalDevice;

	public unsafe Device ( PhysicalDevice physicalDevice, IReadOnlyList<CString> extensions, IReadOnlyList<CString> layers, IEnumerable<QueueFamily> queues ) {
		PhysicalDevice = physicalDevice;
		QueueFamilies = queues.Distinct().ToArray();
		var distinctQueues = QueueFamilies.Select( x => x.Index ).ToArray();

		float priority = 1;
		var queueInfos = new VkDeviceQueueCreateInfo[distinctQueues.Length];
		for ( int i = 0; i < distinctQueues.Length; i++ ) {
			queueInfos[i] = new VkDeviceQueueCreateInfo() {
				sType = VkStructureType.DeviceQueueCreateInfo,
				queueFamilyIndex = distinctQueues[0],
				queueCount = 1,
				pQueuePriorities = &priority
			};
		}

		VkPhysicalDeviceFeatures features = new() {
			samplerAnisotropy = true,
			sampleRateShading = true
		};

		var extensionNames = extensions.MakeArray();
		var layerNames = layers.MakeArray();
		VkDeviceCreateInfo info = new() {
			sType = VkStructureType.DeviceCreateInfo,
			pQueueCreateInfos = queueInfos.Data(),
			queueCreateInfoCount = (uint)queueInfos.Length,
			pEnabledFeatures = &features,
			enabledLayerCount = (uint)layers.Count,
			ppEnabledLayerNames = layerNames.Data(),
			enabledExtensionCount = (uint)extensions.Count,
			ppEnabledExtensionNames = extensionNames.Data()
		};

		Vk.vkCreateDevice( physicalDevice, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	Dictionary<(uint, uint), Queue> queues = new();
	public Queue GetQueue ( QueueFamily family, uint index = 0 ) {
		if ( !queues.TryGetValue( (family.Index, index), out var queue ) ) {
			Vk.vkGetDeviceQueue( this, family.Index, index, out var q );
			queues.Add( (family.Index, index), queue = new(q, this, family) );
		}
		return queue;
	}

	public Swapchain CreateSwapchain ( VkSurfaceKHR surface, SwapchainFormat format, Size2<uint> pixelSize ) {
		return new( this, surface, format, pixelSize, QueueFamilies );
	}

	public ShaderModule CreateShaderModule ( SpirvBytecode bytecode ) {
		return new( this, bytecode );
	}

	public CommandPool CreateCommandPool ( QueueFamily queue, VkCommandPoolCreateFlags flags = VkCommandPoolCreateFlags.ResetCommandBuffer ) {
		return new( this, queue, flags );
	}

	public Semaphore CreateSemaphore () {
		return new Semaphore( this );
	}

	public Fence CreateFence ( bool signaled = false ) {
		return new Fence( this, signaled );
	}

	public void WaitIdle () {
		Vk.vkDeviceWaitIdle( this ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyDevice( Instance, VulkanExtensions.TODO_Allocator );
	}
}
