using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class Device : DisposableObject, IGraphicsDevice {
	public readonly VkDevice Handle;

	public Device ( VkDevice handle ) {
		Handle = handle;
	}

	public unsafe IGpuBarrier CreateGpuBarrier () {
		VkSemaphoreCreateInfo semaphoreInfo = new() {
			sType = VkStructureType.SemaphoreCreateInfo
		};
		VulkanExtensions.Validate( Vk.vkCreateSemaphore( Handle, &semaphoreInfo, VulkanExtensions.TODO_Allocator, out var semaphore ) );

		return new Synchronisation.Semaphore( Handle, semaphore );
	}

	public unsafe ICpuBarrier CreateCpuBarrier ( bool signaled = false ) {
		VkFenceCreateInfo fenceInfo = new() {
			sType = VkStructureType.FenceCreateInfo,
			flags = signaled ? VkFenceCreateFlags.Signaled : VkFenceCreateFlags.None
		};
		VulkanExtensions.Validate( Vk.vkCreateFence( Handle, &fenceInfo, VulkanExtensions.TODO_Allocator, out var fence ) );

		return new Synchronisation.Fence( Handle, fence );
	}

	Dictionary<(uint, uint), Queue> queues = new();
	Dictionary<uint, VkCommandPool> commandPoolsByFamily = new();
	public unsafe Queue GetQueue ( uint familyIndex, uint index = 0 ) {
		if ( !queues.TryGetValue( (familyIndex, index), out var queue ) ) {
			if ( !commandPoolsByFamily.TryGetValue( familyIndex, out var pool ) ) {
				VkCommandPoolCreateInfo poolInfo = new() {
					sType = VkStructureType.CommandPoolCreateInfo,
					flags = VkCommandPoolCreateFlags.ResetCommandBuffer,
					queueFamilyIndex = familyIndex
				};

				VulkanExtensions.Validate( Vk.vkCreateCommandPool( this, &poolInfo, VulkanExtensions.TODO_Allocator, out pool ) );
				commandPoolsByFamily.Add( familyIndex, pool );
			}

			queues.Add( (familyIndex, index), queue = new(this, familyIndex, index, pool) );
		}

		return queue;
	}

	public static implicit operator VkDevice ( Device device ) => device.Handle;
	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var (_, i) in commandPoolsByFamily ) {
			Vk.vkDestroyCommandPool( this, i, VulkanExtensions.TODO_Allocator );
		}
		Vk.vkDestroyDevice( Handle, VulkanExtensions.TODO_Allocator );
	}

	public unsafe IShaderPart CreateShaderPart ( SpirvBytecode spirv ) {
		return new ShaderModule( this, spirv );
	}

	public IShader CreateShader ( IShaderPart[] parts ) {
		return new PipelineShaderInfo( this, parts.Cast<ShaderModule>() );
	}
}
