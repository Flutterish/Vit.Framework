using Vit.Framework.Graphics.Vulkan.Uniforms;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class Pipeline : DisposableVulkanObject<VkPipeline> {
	public readonly VkDevice Device;
	public readonly VkPipelineLayout Layout;
	public unsafe Pipeline ( VkDevice device, PipelineArgs args ) {
		var (shaders, renderPass, depthTest) = (args.Shaders, args.RenderPass, args.DepthTest);
		Device = device;
		var dynamicStates = new[] {
			VkDynamicState.Viewport,
			VkDynamicState.Scissor
		};

		var stateInfo = new VkPipelineDynamicStateCreateInfo() {
			sType = VkStructureType.PipelineDynamicStateCreateInfo,
			dynamicStateCount = (uint)dynamicStates.Length,
			pDynamicStates = dynamicStates.Data()
		};

		var (attribs, sets) = (shaders.Attributes, shaders.AttributeSets);
		var vert = new VkPipelineVertexInputStateCreateInfo() {
			sType = VkStructureType.PipelineVertexInputStateCreateInfo,
			vertexAttributeDescriptionCount = (uint)attribs.Length,
			pVertexAttributeDescriptions = attribs.Data(),
			vertexBindingDescriptionCount = (uint)sets.Length,
			pVertexBindingDescriptions = sets.Data()
		};

		var assembly = new VkPipelineInputAssemblyStateCreateInfo() {
			sType = VkStructureType.PipelineInputAssemblyStateCreateInfo,
			topology = VkPrimitiveTopology.TriangleList,
			primitiveRestartEnable = false
		};

		var viewportInfo = new VkPipelineViewportStateCreateInfo() {
			sType = VkStructureType.PipelineViewportStateCreateInfo,
			viewportCount = 1,
			scissorCount = 1
		};

		var rasterizerInfo = new VkPipelineRasterizationStateCreateInfo() {
			sType = VkStructureType.PipelineRasterizationStateCreateInfo,
			depthClampEnable = false,
			rasterizerDiscardEnable = false,
			polygonMode = VkPolygonMode.Fill,
			lineWidth = 1,
			cullMode = VkCullModeFlags.None,
			frontFace = VkFrontFace.Clockwise,
			depthBiasEnable = false
		};

		var multisampleInfo = new VkPipelineMultisampleStateCreateInfo() {
			sType = VkStructureType.PipelineMultisampleStateCreateInfo,
			sampleShadingEnable = true,
			rasterizationSamples = renderPass.Samples,
			minSampleShading = 1
		};

		var blendInfo = new VkPipelineColorBlendAttachmentState() {
			colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
			blendEnable = false
		};

		var colorInfo = new VkPipelineColorBlendStateCreateInfo() {
			sType = VkStructureType.PipelineColorBlendStateCreateInfo,
			logicOpEnable = false,
			attachmentCount = 1,
			pAttachments = &blendInfo
		};
		var depthInfo = new VkPipelineDepthStencilStateCreateInfo() {
			sType = VkStructureType.PipelineDepthStencilStateCreateInfo,
			depthTestEnable = depthTest.IsEnabled,
			depthWriteEnable = depthTest.WriteOnPass,
			depthCompareOp = depthTest.CompareOperation.CompareOp(),
			depthBoundsTestEnable = false,
			stencilTestEnable = false
		};

		var uniforms = shaders.UniformSets.Select( x => x.Layout ).ToArray();
		var layoutInfo = new VkPipelineLayoutCreateInfo() {
			sType = VkStructureType.PipelineLayoutCreateInfo,
			setLayoutCount = (uint)uniforms.Length,
			pSetLayouts = uniforms.Data()
		};

		Vk.vkCreatePipelineLayout( Device, &layoutInfo, VulkanExtensions.TODO_Allocator, out Layout ).Validate();

		var stages = shaders.Modules.Select( x => x.StageCreateInfo with { pName = x.EntryPoint } ).ToArray();
		var info = new VkGraphicsPipelineCreateInfo() {
			sType = VkStructureType.GraphicsPipelineCreateInfo,
			stageCount = (uint)shaders.Modules.Length,
			pStages = stages.Data(),
			pVertexInputState = &vert,
			pInputAssemblyState = &assembly,
			pViewportState = &viewportInfo,
			pRasterizationState = &rasterizerInfo,
			pMultisampleState = &multisampleInfo,
			pColorBlendState = &colorInfo,
			pDepthStencilState = &depthInfo,
			pDynamicState = &stateInfo,
			layout = Layout,
			renderPass = renderPass,
			subpass = 0
		};

		Vk.vkCreateGraphicsPipelines( Device, VkPipelineCache.Null, 1, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
	}

	protected override unsafe void Dispose ( bool disposing ) {
		Vk.vkDestroyPipeline( Device, Instance, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyPipelineLayout( Device, Layout, VulkanExtensions.TODO_Allocator );
	}
}
