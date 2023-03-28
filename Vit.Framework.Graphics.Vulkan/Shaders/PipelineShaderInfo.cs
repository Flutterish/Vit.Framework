using System.Collections.Immutable;
using Vit.Framework.Allocation;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Shaders;

public class PipelineShaderInfo : DisposableObject, IShader {
	public readonly VkPipelineLayout Layout;
	public readonly VkPipeline BasePipeline;
	public readonly VkDevice Device;
	public readonly VkPipelineVertexInputStateCreateInfo VertexFormat;
	public readonly ImmutableArray<VkPipelineShaderStageCreateInfo> Stages;

	public unsafe PipelineShaderInfo ( VkDevice device, IEnumerable<ShaderModule> modules ) {
		Device = device;

		var vertexFormat = VertexFormat = new() { // TODO construct with refletions via spirv-cross
			sType = VkStructureType.PipelineVertexInputStateCreateInfo,
			vertexBindingDescriptionCount = 0,
			vertexAttributeDescriptionCount = 0
		};

		Stages = modules.Select( x => x.CreateInfo ).ToImmutableArray();

		VkPipelineLayoutCreateInfo layoutInfo = new() {
			sType = VkStructureType.PipelineLayoutCreateInfo
		};
		VulkanExtensions.Validate( Vk.vkCreatePipelineLayout( device, &layoutInfo, VulkanExtensions.TODO_Allocator, out Layout ) );

		VkGraphicsPipelineCreateInfo pipelineInfo = new() {
			sType = VkStructureType.GraphicsPipelineCreateInfo,
			stageCount = (uint)Stages.Length,
			pStages = Stages.Data(),
			pVertexInputState = &vertexFormat,
			//pInputAssemblyState = &inputInfo,
			//pViewportState = &viewportInfo,
			//pRasterizationState = &rasterInfo,
			//pMultisampleState = &multisampleInfo,
			//pColorBlendState = &blendInfo,
			//pDynamicState = &dynamicStateInfo,
			layout = Layout//,
			//renderPass = renderPass
		};

		VulkanExtensions.Validate( Vk.vkCreateGraphicsPipelines( device, 0, 1, &pipelineInfo, VulkanExtensions.TODO_Allocator, out pipeline ) );
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyPipeline( Device, BasePipeline, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyPipelineLayout( Device, Layout, VulkanExtensions.TODO_Allocator );
	}
}
