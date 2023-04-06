using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class ShaderModule : DisposableVulkanObject<VkShaderModule> {
	public readonly VkDevice Device;
	public readonly VkPipelineShaderStageCreateInfo StageCreateInfo;
	public readonly CString EntryPoint;
	public unsafe ShaderModule ( VkDevice device, SpirvBytecode bytecode ) {
		Device = device;
		VkShaderModuleCreateInfo info = new() {
			sType = VkStructureType.ShaderModuleCreateInfo,
			codeSize = (uint)bytecode.Data.Length,
			pCode = MemoryMarshal.Cast<byte, uint>( bytecode.Data ).Data()
		};

		Vk.vkCreateShaderModule( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();

		EntryPoint = bytecode.EntryPoint;
		StageCreateInfo = new() {
			sType = VkStructureType.PipelineShaderStageCreateInfo,
			stage = bytecode.Type switch {
				ShaderPartType.Vertex => VkShaderStageFlags.Vertex,
				ShaderPartType.Fragment => VkShaderStageFlags.Fragment,
				_ => throw new InvalidOperationException( $"Shader stage not supported: {bytecode.Type}" )
			},
			module = Instance,
			pName = EntryPoint
		};
	}

	public VkPipelineVertexInputStateCreateInfo GetVertexInfo () {
		return new() {
			sType = VkStructureType.PipelineVertexInputStateCreateInfo
		};
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyShaderModule( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
