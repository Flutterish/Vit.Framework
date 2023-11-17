using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Interop;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class Pipeline : DisposableVulkanObject<VkPipeline> {
	public readonly VkDevice Device;
	public readonly VkPipelineLayout Layout;
	public unsafe Pipeline ( VkDevice device, PipelineArgs args ) {
		var (shaders, renderPass, depthTest, depthState, stencilTest, stencilState) = (args.Shaders, args.RenderPass, args.DepthTest, args.DepthState, args.StencilTest, args.StencilState);
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

		static VkBlendOp op ( BlendFunction func ) {
			return func switch {
				BlendFunction.Add => VkBlendOp.Add,
				BlendFunction.Max => VkBlendOp.Max,
				BlendFunction.Min => VkBlendOp.Min,
				BlendFunction.FragmentMinusDestination => VkBlendOp.Subtract,
				BlendFunction.DestinationMinusFragment or _ => VkBlendOp.ReverseSubtract
			};
		}

		static VkBlendFactor factor ( BlendFactor factor ) {
			return factor switch {
				BlendFactor.Zero => VkBlendFactor.Zero,
				BlendFactor.One => VkBlendFactor.One,
				BlendFactor.Fragment => VkBlendFactor.SrcColor,
				BlendFactor.FragmentInverse => VkBlendFactor.OneMinusSrcColor,
				BlendFactor.Destination => VkBlendFactor.DstColor,
				BlendFactor.DestinationInverse => VkBlendFactor.OneMinusDstColor,
				BlendFactor.FragmentAlpha => VkBlendFactor.SrcAlpha,
				BlendFactor.FragmentAlphaInverse => VkBlendFactor.OneMinusSrcAlpha,
				BlendFactor.DestinationAlpha => VkBlendFactor.DstAlpha,
				BlendFactor.DestinationAlphaInverse => VkBlendFactor.OneMinusDstAlpha,
				BlendFactor.Constant => VkBlendFactor.ConstantColor,
				BlendFactor.ConstantInverse => VkBlendFactor.OneMinusConstantColor,
				BlendFactor.ConstantAlpha => VkBlendFactor.ConstantAlpha,
				BlendFactor.ConstantAlphaInverse => VkBlendFactor.OneMinusConstantAlpha,
				BlendFactor.AlphaSaturate => VkBlendFactor.SrcAlphaSaturate,
				BlendFactor.SecondFragment => VkBlendFactor.Src1Color,
				BlendFactor.SecondFragmentInverse => VkBlendFactor.OneMinusSrc1Color,
				BlendFactor.SecondFragmentAlpha => VkBlendFactor.Src1Alpha,
				BlendFactor.SecondFragmentAlphaInverse or _ => VkBlendFactor.OneMinusSrc1Alpha
			};
		}

		var blendInfo = args.BlendState.IsEnabled 
		? new VkPipelineColorBlendAttachmentState() {
			colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
			blendEnable = true,
			alphaBlendOp = op( args.BlendState.AlphaFunction ),
			colorBlendOp = op( args.BlendState.ColorFunction ),
			srcColorBlendFactor = factor( args.BlendState.FragmentColorFactor ),
			srcAlphaBlendFactor = factor( args.BlendState.FragmentAlphaFactor ),
			dstColorBlendFactor = factor( args.BlendState.DestinationColorFactor ),
			dstAlphaBlendFactor = factor( args.BlendState.DestinationAlphaFactor )
		} 
		: new VkPipelineColorBlendAttachmentState() {
			colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
			blendEnable = false
		};

		var colorInfo = new VkPipelineColorBlendStateCreateInfo() {
			sType = VkStructureType.PipelineColorBlendStateCreateInfo,
			logicOpEnable = false,
			attachmentCount = 1,
			pAttachments = &blendInfo,
			blendConstants_0 = args.BlendState.Constant.X,
			blendConstants_1 = args.BlendState.Constant.Y,
			blendConstants_2 = args.BlendState.Constant.Z,
			blendConstants_3 = args.BlendState.Constant.W
		};

		VkStencilOpState stencilOpState = new() {
			compareMask = stencilState.CompareMask,
			compareOp = stencilTest.CompareOperation.CompareOp(),
			reference = stencilState.ReferenceValue,
			writeMask = stencilState.WriteMask,
			passOp = stencilState.PassOperation.StencilOp(),
			depthFailOp = stencilState.DepthFailOperation.StencilOp(),
			failOp = stencilState.StencilFailOperation.StencilOp()
		};
		var depthStencilInfo = new VkPipelineDepthStencilStateCreateInfo() {
			sType = VkStructureType.PipelineDepthStencilStateCreateInfo,
			depthTestEnable = depthTest.IsEnabled,
			depthWriteEnable = depthState.WriteOnPass,
			depthCompareOp = depthTest.CompareOperation.CompareOp(),
			depthBoundsTestEnable = false,
			stencilTestEnable = stencilTest.IsEnabled,
			back = stencilOpState,
			front = stencilOpState
		};

		var uniforms = shaders.UniformSets.Select( x => x!.Layout ).ToArray();
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
			pDepthStencilState = &depthStencilInfo,
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
