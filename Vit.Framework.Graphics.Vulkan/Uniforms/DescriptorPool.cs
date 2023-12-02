using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Uniforms;

public class DescriptorPool : DisposableVulkanObject<VkDescriptorPool>, IUniformSetPool {
	public Device Device => Layout.Device;
	public readonly DescriptorSetLayout Layout;
	public unsafe DescriptorPool ( DescriptorSetLayout layout, uint size ) {
		Layout = layout;
		var layoutBindings = layout.LayoutBindings;

		var values = new VkDescriptorPoolSize[layoutBindings.Length];
		for ( int i = 0; i < values.Length; i++ ) {
			values[i] = new() {
				type = layoutBindings[i].descriptorType,
				descriptorCount = 1
			};
		}

		fixed ( VkDescriptorPoolSize* valuesPtr = values ) {
			var info = new VkDescriptorPoolCreateInfo() {
				sType = VkStructureType.DescriptorPoolCreateInfo,
				poolSizeCount = (uint)values.Length,
				pPoolSizes = valuesPtr,
				maxSets = size
			};
			Vk.vkCreateDescriptorPool( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		}

		created = new( (int)size );
	}

	public DescriptorSet CreateSet () {
		var value = new DescriptorSet( this, Layout );
		DebugMemoryAlignment.SetDebugData( value, Layout.Type.Resources );
		return value;
	}

	Stack<IUniformSet> created;
	public IUniformSet Rent () {
		if ( !created.TryPop( out var set ) )
			set = CreateSet();

		return set;
	}

	public void Free ( IUniformSet set ) {
		created.Push( set );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyDescriptorPool( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
