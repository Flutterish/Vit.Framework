using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class ShaderModule : DisposableObject, IShaderPart
{
    public readonly VkDevice Device;
    public readonly VkShaderModule Handle;
    public readonly VkPipelineShaderStageCreateInfo CreateInfo;
    public readonly VkShaderStageFlags Type;

	SpirvBytecode spirv;
    CString entryPoint;
    public unsafe ShaderModule(VkDevice device, SpirvBytecode spirv)
    {
        Device = device;
        this.spirv = spirv;

        var data = spirv.Data;
        VkShaderModuleCreateInfo info = new()
        {
            sType = VkStructureType.ShaderModuleCreateInfo,
            codeSize = (uint)data.Length,
            pCode = (uint*)Unsafe.AsPointer(ref MemoryMarshal.AsRef<uint>(data))
        };

        VulkanExtensions.Validate(Vk.vkCreateShaderModule(Device, &info, VulkanExtensions.TODO_Allocator, out Handle));

        CreateInfo = new()
        {
            sType = VkStructureType.PipelineShaderStageCreateInfo,
            stage = Type = spirv.Type switch { 
                ShaderPartType.Fragment => VkShaderStageFlags.Fragment,
                ShaderPartType.Vertex => VkShaderStageFlags.Vertex,
                _ => throw new InvalidOperationException( $"Shader type not supported: {spirv.Type}" )
			},
            module = Handle,
            pName = entryPoint = spirv.EntryPoint
        };
    }

    protected override unsafe void Dispose(bool disposing)
    {
        Vk.vkDestroyShaderModule(Device, Handle, VulkanExtensions.TODO_Allocator);
    }
}
