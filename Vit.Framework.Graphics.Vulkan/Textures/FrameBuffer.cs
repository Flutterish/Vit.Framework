using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Textures;

public class FrameBuffer : DisposableObject, IFrameBuffer
{
    public readonly VkFramebuffer Handle;
    public readonly VkDevice Device;
    public FrameBuffer(VkDevice device, VkFramebuffer handle)
    {
        Handle = handle;
        Device = device;
    }

    protected override unsafe void Dispose(bool disposing)
    {
        Vk.vkDestroyFramebuffer(Device, Handle, VulkanExtensions.TODO_Allocator);
    }
}
