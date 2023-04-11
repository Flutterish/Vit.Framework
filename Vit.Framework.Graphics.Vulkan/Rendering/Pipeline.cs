using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class Pipeline : DisposableVulkanObject<VkPipeline> {
	public readonly VkDevice Device;
	public readonly VkPipelineLayout Layout;
	public readonly VkDescriptorSetLayout Uniforms;
	public readonly DescriptorPool DescriptorPool;
	public readonly DescriptorSet DescriptorSet;
	public unsafe Pipeline ( VkDevice device, ShaderModule[] shaders, RenderPass renderPass ) {
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

		var vert = shaders.First( x => x.StageCreateInfo.stage.HasFlag( VkShaderStageFlags.Vertex ) ).GetVertexInfo();
		var vertexBinding = new VkVertexInputBindingDescription() { // TODO this is hardcoded
			binding = 0,
			stride = sizeof( float ) * 7,
			inputRate = VkVertexInputRate.Vertex
		};
		var vertexAttributes = new VkVertexInputAttributeDescription[] {
			new() {
				binding = 0,
				location = 0,
				format = VkFormat.R32g32Sfloat,
				offset = 0
			},
			new() {
				binding = 0,
				location = 1,
				format = VkFormat.R32g32b32Sfloat,
				offset = sizeof(float) * 2
			},
			new() {
				binding = 0,
				location = 2,
				format = VkFormat.R32g32Sfloat,
				offset = sizeof(float) * 5
			}
		};
		vert.vertexBindingDescriptionCount = 1;
		vert.vertexAttributeDescriptionCount = (uint)vertexAttributes.Length;
		vert.pVertexBindingDescriptions = &vertexBinding;
		vert.pVertexAttributeDescriptions = vertexAttributes.Data();

		var uniforms = new VkDescriptorSetLayoutBinding() { // TODO this is hardcoded
			binding = 0,
			descriptorCount = 1,
			descriptorType = VkDescriptorType.UniformBuffer,
			stageFlags = VkShaderStageFlags.Vertex
		};
		var samplers = new VkDescriptorSetLayoutBinding() {
			binding = 1,
			descriptorCount = 1,
			descriptorType = VkDescriptorType.CombinedImageSampler,
			stageFlags = VkShaderStageFlags.Fragment
		};
		var layouts = new[] { uniforms, samplers };
		var uniformInfo = new VkDescriptorSetLayoutCreateInfo() {
			sType = VkStructureType.DescriptorSetLayoutCreateInfo,
			bindingCount = (uint)layouts.Length,
			pBindings = layouts.Data()
		};
		Vk.vkCreateDescriptorSetLayout( Device, &uniformInfo, VulkanExtensions.TODO_Allocator, out this.Uniforms ).Validate();
		DescriptorPool = new( device, 
			new VkDescriptorPoolSize() { type = VkDescriptorType.UniformBuffer, descriptorCount = 1 },
			new VkDescriptorPoolSize() { type = VkDescriptorType.CombinedImageSampler, descriptorCount = 1 } 
		);
		DescriptorSet = DescriptorPool.CreateSet( this.Uniforms );

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
			depthBiasEnable = false
		};

		var multisampleInfo = new VkPipelineMultisampleStateCreateInfo() {
			sType = VkStructureType.PipelineMultisampleStateCreateInfo,
			sampleShadingEnable = false,
			rasterizationSamples = VkSampleCountFlags.Count1
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

		var unifomsHandle = this.Uniforms;
		var layoutInfo = new VkPipelineLayoutCreateInfo() {
			sType = VkStructureType.PipelineLayoutCreateInfo,
			setLayoutCount = 1,
			pSetLayouts = &unifomsHandle
		};

		Vk.vkCreatePipelineLayout( Device, &layoutInfo, VulkanExtensions.TODO_Allocator, out Layout ).Validate();

		var stages = shaders.Select( x => x.StageCreateInfo ).ToArray();
		var info = new VkGraphicsPipelineCreateInfo() {
			sType = VkStructureType.GraphicsPipelineCreateInfo,
			stageCount = (uint)shaders.Length,
			pStages = stages.Data(),
			pVertexInputState = &vert,
			pInputAssemblyState = &assembly,
			pViewportState = &viewportInfo,
			pRasterizationState = &rasterizerInfo,
			pMultisampleState = &multisampleInfo,
			pColorBlendState = &colorInfo,
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
		Vk.vkDestroyDescriptorPool( Device, DescriptorPool, VulkanExtensions.TODO_Allocator );
		Vk.vkDestroyDescriptorSetLayout( Device, Uniforms, VulkanExtensions.TODO_Allocator );
	}
}
