using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class ShaderModule : DisposableVulkanObject<VkShaderModule>, IShaderPart {
	public readonly Device Device;
	public readonly VkPipelineShaderStageCreateInfo StageCreateInfo;
	public readonly CString EntryPoint;
	public readonly SpirvBytecode Spirv;

	public ShaderPartType Type => ShaderInfo.Type;
	public ShaderInfo ShaderInfo => Spirv.Reflections;

	public unsafe ShaderModule ( Device device, SpirvBytecode bytecode ) {
		Spirv = bytecode;
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
			stage = FlagsFromPartType( bytecode.Type ),
			module = Instance,
			pName = EntryPoint
		};
	}

	public static VkShaderStageFlags FlagsFromPartType ( ShaderPartType type ) {
		return type switch {
			ShaderPartType.Vertex => VkShaderStageFlags.Vertex,
			ShaderPartType.Fragment => VkShaderStageFlags.Fragment,
			ShaderPartType.Compute => VkShaderStageFlags.Compute,
			_ => throw new InvalidOperationException( $"Shader stage not supported: {type}" )
		};
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyShaderModule( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
