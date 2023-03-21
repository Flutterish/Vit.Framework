﻿using SDL2;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Vulkan;
using Vit.Framework.Threading;
using Vortice.ShaderCompiler;
using Vulkan;
using Vk = Vulkan.VulkanNative;

namespace Vit.Framework.Windowing.Sdl;

unsafe class SdlVulkanRenderThread : AppThread {
	SdlWindow window;
	public SdlVulkanRenderThread ( SdlWindow window, string name ) : base( name ) {
		this.window = window;
	}

	VulkanInstance vulkan = null!;
	VkSurfaceKHR surface;
	VkDevice device;
	VkQueue graphicsQueue;
	VkQueue presentQueue;
	VkSwapchainKHR swapchain;
	VkFormat swapChainFormat;
	VkExtent2D swapchainSize;
	VkImage[] images = null!;
	VkImageView[] imageViews = null!;
	VkPipelineLayout pipelineLayout;
	VkRenderPass renderPass;
	VkPipeline pipeline;
	VkFramebuffer[] framebuffers = null!;
	VkCommandPool commnadPool;
	VkCommandBuffer commandBuffer;
	VkClearValue clearValue;
	CString[] requiredDeviceExtensions = { "VK_KHR_swapchain" };
	CString[] validationLayers = { "VK_LAYER_KHRONOS_validation" };
	protected override void Initialize () {
		vulkan = new VulkanInstance( ( string[] availableExtensions, out CString[] selected ) => {
			SDL.SDL_Vulkan_GetInstanceExtensions( window.Pointer, out var count, null );
			nint[] pointers = new nint[count];
			SDL.SDL_Vulkan_GetInstanceExtensions( window.Pointer, out count, pointers );
			selected = pointers.Select( x => new CString( x ) ).ToArray();

			return !selected.Select( x => x.ToString() ).Except( availableExtensions ).Any();
		}, ( string[] availableLayers, out CString[] selected ) => {
			selected = validationLayers;
			if ( selected.Select( x => x.ToString() ).Except( availableLayers ).Any() )
				throw new Exception( "Not all required validation layers are present in a debug build" );

			return true;
		} );

		surface = createSurface();
		var (physicalDevice, swapchain) = vulkan.GetAllPhysicalDevices().Where( x => {
			return !requiredDeviceExtensions.Select( x => x.ToString() ).Except( x.Extensions ).Any()
				&& x.QueuesByCapabilities.ContainsKey( VkQueueFlags.Graphics )
				&& x.QueueIndices.Any( i => x.QueueSupportsSurface( surface, i ) );
		} ).Select( x => (device: x, swapchain: x.GetSwapChainDetails(surface)) ).Where( x => {
			return x.swapchain.Formats.Any() && x.swapchain.PresentModes.Any();
		} ).OrderBy( x => x.device.Properties.deviceType switch {
			VkPhysicalDeviceType.DiscreteGpu => 1,
			VkPhysicalDeviceType.IntegratedGpu => 2,
			_ => 3
		} ).First();

		var format = swapchain.Formats.OrderBy( x => x switch { 
			{ format: VkFormat.B8g8r8a8Srgb, colorSpace: VkColorSpaceKHR.SrgbNonlinearKHR } => 1,
			_ => 2
		} ).First();
		var presentMode = swapchain.PresentModes.OrderBy( x => x switch {
			VkPresentModeKHR.MailboxKHR => 1,
			VkPresentModeKHR.FifoKHR => 9,
			VkPresentModeKHR.FifoRelaxedKHR => 10,
			VkPresentModeKHR.ImmediateKHR or _ => 11
		} ).First();
		swapChainFormat = format.format;

		swapchainSize = getSwapSize( swapchain.Capabilities );
		uint imageCount = Math.Min( 
			swapchain.Capabilities.minImageCount + 1, 
			swapchain.Capabilities.maxImageCount == 0 ? uint.MaxValue : swapchain.Capabilities.maxImageCount  
		);

		var queues = new uint[] {
			physicalDevice.QueuesByCapabilities[VkQueueFlags.Graphics][0],
			physicalDevice.QueueIndices.First( i => physicalDevice.QueueSupportsSurface(surface, i) )
		};

		device = physicalDevice.CreateLogicalDevice( requiredDeviceExtensions, queues );
		Vk.vkGetDeviceQueue( device, queues[0], 0, out graphicsQueue );
		Vk.vkGetDeviceQueue( device, queues[1], 0, out presentQueue );

		var swapchainInfo = new VkSwapchainCreateInfoKHR() {
			sType = VkStructureType.SwapchainCreateInfoKHR,
			minImageCount = imageCount,
			imageFormat = format.format,
			imageColorSpace = format.colorSpace,
			imageExtent = swapchainSize,
			imageArrayLayers = 1,
			imageUsage = VkImageUsageFlags.ColorAttachment,
			preTransform = swapchain.Capabilities.currentTransform,
			compositeAlpha = VkCompositeAlphaFlagsKHR.OpaqueKHR,
			presentMode = presentMode,
			clipped = true,
			surface = surface
		};

		if ( graphicsQueue != presentQueue ) {
			swapchainInfo.imageSharingMode = VkSharingMode.Concurrent;
			swapchainInfo.queueFamilyIndexCount = 2;
			swapchainInfo.pQueueFamilyIndices = queues.Data();
		}
		else {
			swapchainInfo.imageSharingMode = VkSharingMode.Exclusive;
		}

		VulkanExtensions.Validate( Vk.vkCreateSwapchainKHR( device, &swapchainInfo, vulkan.Allocator, out this.swapchain ) );

		images = VulkanExtensions.Out<VkImage>.Enumerate( device, this.swapchain, Vk.vkGetSwapchainImagesKHR );
		imageViews = images.Select( x => {
			VkImageViewCreateInfo info = new() {
				sType = VkStructureType.ImageViewCreateInfo,
				image = x,
				viewType = VkImageViewType.Image2D,
				format = swapChainFormat,
				components = new() {
					r = VkComponentSwizzle.Identity,
					g = VkComponentSwizzle.Identity,
					b = VkComponentSwizzle.Identity,
					a = VkComponentSwizzle.Identity
				},
				subresourceRange = new() {
					aspectMask = VkImageAspectFlags.Color,
					baseMipLevel = 0,
					levelCount = 1,
					baseArrayLayer = 0,
					layerCount = 1
				}
			};

			VulkanExtensions.Validate( Vk.vkCreateImageView( device, &info, vulkan.Allocator, out var view ) );
			return view;
		} ).ToArray();

		initPipeline();

		framebuffers = imageViews.Select( x => {
			VkFramebufferCreateInfo info = new() {
				sType = VkStructureType.FramebufferCreateInfo,
				renderPass = renderPass,
				attachmentCount = 1,
				pAttachments = &x,
				width = swapchainSize.width,
				height = swapchainSize.height,
				layers = 1
			};

			VulkanExtensions.Validate( Vk.vkCreateFramebuffer( device, &info, vulkan.Allocator, out var fb ) );
			return fb;
		} ).ToArray();

		VkCommandPoolCreateInfo poolInfo = new() {
			sType = VkStructureType.CommandPoolCreateInfo,
			flags = VkCommandPoolCreateFlags.ResetCommandBuffer,
			queueFamilyIndex = queues[0]
		};

		VulkanExtensions.Validate( Vk.vkCreateCommandPool( device, &poolInfo, vulkan.Allocator, out commnadPool ) );

		VkCommandBufferAllocateInfo bufferInfo = new() {
			sType = VkStructureType.CommandBufferAllocateInfo,
			commandPool = commnadPool,
			level = VkCommandBufferLevel.Primary,
			commandBufferCount = 1
		};

		VulkanExtensions.Validate( Vk.vkAllocateCommandBuffers( device, &bufferInfo, out commandBuffer ) );

		var rng = new Random();
		clearValue = new() {
			color = new VkClearColorValue( rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1 )
		};

		VkSemaphoreCreateInfo semaphoreInfo = new() {
			sType = VkStructureType.SemaphoreCreateInfo
		};
		VkFenceCreateInfo fenceInfo = new() {
			sType = VkStructureType.FenceCreateInfo
		};
		VulkanExtensions.Validate( Vk.vkCreateSemaphore( device, &semaphoreInfo, vulkan.Allocator, out imageAvailableSemaphore ) );
		VulkanExtensions.Validate( Vk.vkCreateSemaphore( device, &semaphoreInfo, vulkan.Allocator, out renderFinishedSemaphore ) );
		VulkanExtensions.Validate( Vk.vkCreateFence( device, &fenceInfo, vulkan.Allocator, out inFlightFence ) );
	}

	void initPipeline () {
		Options options = new();
		options.SetSourceLanguage( SourceLanguage.GLSL );
		var compiler = new Compiler(options);

		var vs = compiler.Compile( @"#version 450
			layout(location = 0) out vec3 fragColor;

			vec2 positions[3] = vec2[](
				vec2(0.0, -0.5),
				vec2(0.5, 0.5),
				vec2(-0.5, 0.5)
			);

			vec3 colors[3] = vec3[](
				vec3(1.0, 0.0, 0.0),
				vec3(0.0, 1.0, 0.0),
				vec3(0.0, 0.0, 1.0)
			);

			void main() {
				gl_Position = vec4(positions[gl_VertexIndex], 0.0, 1.0);
				fragColor = colors[gl_VertexIndex];
			}", "", ShaderKind.VertexShader
		);
		var fs = compiler.Compile( @"#version 450
			layout(location = 0) in vec3 fragColor;
			layout(location = 0) out vec4 outColor;

			void main() {
				outColor = vec4(fragColor, 1.0);
			}", "", ShaderKind.FragmentShader
		);

		var vsModule = createShader( vs.GetBytecode() );
		var fsModule = createShader( fs.GetBytecode() );

		VkPipelineShaderStageCreateInfo vertInfo = new() {
			sType = VkStructureType.PipelineShaderStageCreateInfo,
			stage = VkShaderStageFlags.Vertex,
			module = vsModule,
			pName = (CString)"main"
		};
		VkPipelineShaderStageCreateInfo fragInfo = new() {
			sType = VkStructureType.PipelineShaderStageCreateInfo,
			stage = VkShaderStageFlags.Fragment,
			module = fsModule,
			pName = (CString)"main"
		};

		var stages = new VkPipelineShaderStageCreateInfo[] { vertInfo, fragInfo };

		VkPipelineVertexInputStateCreateInfo vertexFormat = new() {
			sType = VkStructureType.PipelineVertexInputStateCreateInfo,
			vertexBindingDescriptionCount = 0,
			vertexAttributeDescriptionCount = 0
		};

		VkPipelineInputAssemblyStateCreateInfo inputInfo = new() {
			sType = VkStructureType.PipelineInputAssemblyStateCreateInfo,
			topology = VkPrimitiveTopology.TriangleList,
			primitiveRestartEnable = false
		};

		VkDynamicState[] dynamicState = {
			VkDynamicState.Viewport,
			VkDynamicState.Scissor
		};

		VkPipelineDynamicStateCreateInfo dynamicStateInfo = new() {
			sType = VkStructureType.PipelineDynamicStateCreateInfo,
			dynamicStateCount = (uint)dynamicState.Length,
			pDynamicStates = dynamicState.Data()
		};

		VkPipelineViewportStateCreateInfo viewportInfo = new() {
			sType = VkStructureType.PipelineViewportStateCreateInfo,
			viewportCount = 1,
			scissorCount = 1
		};

		VkPipelineRasterizationStateCreateInfo rasterInfo = new() {
			sType = VkStructureType.PipelineRasterizationStateCreateInfo,
			depthClampEnable = false,
			rasterizerDiscardEnable = false,
			polygonMode = VkPolygonMode.Fill,
			lineWidth = 1,
			cullMode = VkCullModeFlags.None,
			frontFace = VkFrontFace.Clockwise,
			depthBiasEnable = false
		};

		VkPipelineMultisampleStateCreateInfo multisampleInfo = new() {
			sType = VkStructureType.PipelineMultisampleStateCreateInfo,
			sampleShadingEnable = false,
			rasterizationSamples = VkSampleCountFlags.Count1,
			minSampleShading = 1
		};

		VkPipelineColorBlendAttachmentState blend = new() {
			colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A,
			blendEnable = false
		};

		VkPipelineColorBlendStateCreateInfo blendInfo = new() {
			sType = VkStructureType.PipelineColorBlendStateCreateInfo,
			logicOpEnable = false,
			attachmentCount = 1,
			pAttachments = &blend
		};

		VkAttachmentDescription colorAttachment = new() {
			format = swapChainFormat,
			samples = VkSampleCountFlags.Count1,
			loadOp = VkAttachmentLoadOp.Clear,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.PresentSrcKHR
		};

		VkAttachmentReference colorAttachmentRef = new() {
			attachment = 0,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		VkSubpassDescription subpass = new() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = 1,
			pColorAttachments = &colorAttachmentRef
		};

		VkPipelineLayoutCreateInfo layoutInfo = new() {
			sType = VkStructureType.PipelineLayoutCreateInfo
		};

		VkSubpassDependency dependency = new() {
			srcSubpass = ~0u,
			dstSubpass = 0,
			srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			srcAccessMask = 0,
			dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput,
			dstAccessMask = VkAccessFlags.ColorAttachmentWrite
		};

		VkRenderPassCreateInfo renderPassInfo = new() {
			sType = VkStructureType.RenderPassCreateInfo,
			attachmentCount = 1,
			pAttachments = &colorAttachment,
			subpassCount = 1,
			pSubpasses = &subpass,
			dependencyCount = 1,
			pDependencies = &dependency
		};

		VulkanExtensions.Validate( Vk.vkCreateRenderPass( device, &renderPassInfo, vulkan.Allocator, out renderPass ) );
		VulkanExtensions.Validate( Vk.vkCreatePipelineLayout( device, &layoutInfo, vulkan.Allocator, out pipelineLayout ) );

		VkGraphicsPipelineCreateInfo pipelineInfo = new() {
			sType = VkStructureType.GraphicsPipelineCreateInfo,
			stageCount = 2,
			pStages = stages.Data(),
			pVertexInputState = &vertexFormat,
			pInputAssemblyState = &inputInfo,
			pViewportState = &viewportInfo,
			pRasterizationState = &rasterInfo,
			pMultisampleState = &multisampleInfo,
			pColorBlendState = &blendInfo,
			pDynamicState = &dynamicStateInfo,
			layout = pipelineLayout,
			renderPass = renderPass,
			subpass = 0
		};

		VulkanExtensions.Validate( Vk.vkCreateGraphicsPipelines( device, 0, 1, &pipelineInfo, vulkan.Allocator, out pipeline ) );

		Vk.vkDestroyShaderModule( device, vsModule, vulkan.Allocator );
		Vk.vkDestroyShaderModule( device, fsModule, vulkan.Allocator );
	}

	VkSurfaceKHR createSurface () {
		SDL.SDL_Vulkan_CreateSurface( window.Pointer, vulkan.Instance.Handle, out var surfacePtr );
		return new( (ulong)surfacePtr );
	}

	VkExtent2D getSwapSize ( VkSurfaceCapabilitiesKHR capabilities ) {
		if ( capabilities.currentExtent.width != uint.MaxValue ) {
			return capabilities.currentExtent;
		}

		SDL.SDL_Vulkan_GetDrawableSize( window.Pointer, out int pixelWidth, out int pixelHeight );
		return new() {
			width = Math.Clamp( (uint)pixelWidth, capabilities.minImageExtent.width, capabilities.maxImageExtent.width ),
			height = Math.Clamp( (uint)pixelHeight, capabilities.minImageExtent.height, capabilities.maxImageExtent.height )
		};
	}

	VkShaderModule createShader ( Span<byte> spirv ) {
		VkShaderModuleCreateInfo info = new() {
			sType = VkStructureType.ShaderModuleCreateInfo,
			codeSize = (uint)spirv.Length,
			pCode = (uint*)Unsafe.AsPointer( ref MemoryMarshal.AsRef<uint>( spirv ) )
		};

		VulkanExtensions.Validate( Vk.vkCreateShaderModule( device, &info, vulkan.Allocator, out var shader ) );
		return shader;
	}

	VkSemaphore imageAvailableSemaphore;
	VkSemaphore renderFinishedSemaphore;
	VkFence inFlightFence;
	protected override void Loop () {
		VkSemaphore imageAvailableSemaphore = this.imageAvailableSemaphore;
		VkSemaphore renderFinishedSemaphore = this.renderFinishedSemaphore;
		VkFence inFlightFence = this.inFlightFence;
		VkCommandBuffer commandBuffer = this.commandBuffer;
		VkSwapchainKHR swapchain = this.swapchain;

		uint index = 0;
		Vk.vkAcquireNextImageKHR( device, swapchain, ulong.MaxValue, imageAvailableSemaphore, VkFence.Null, ref index );
		Vk.vkResetCommandBuffer( commandBuffer, 0 );
		record( commandBuffer, index );

		VkPipelineStageFlags flags = VkPipelineStageFlags.ColorAttachmentOutput;
		VkSubmitInfo submitInfo = new() { 
			sType = VkStructureType.SubmitInfo,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &imageAvailableSemaphore,
			pWaitDstStageMask = &flags,
			commandBufferCount = 1,
			pCommandBuffers = &commandBuffer,
			signalSemaphoreCount = 1,
			pSignalSemaphores = &renderFinishedSemaphore
		};

		VulkanExtensions.Validate( Vk.vkQueueSubmit( graphicsQueue, 1, &submitInfo, inFlightFence ) );

		VkPresentInfoKHR presentInfo = new() {
			sType = VkStructureType.PresentInfoKHR,
			waitSemaphoreCount = 1,
			pWaitSemaphores = &renderFinishedSemaphore,
			swapchainCount = 1,
			pSwapchains = &swapchain,
			pImageIndices = &index
		};

		Vk.vkQueuePresentKHR( presentQueue, &presentInfo );

		Vk.vkWaitForFences( device, 1, &inFlightFence, true, ulong.MaxValue );
		Vk.vkResetFences( device, 1, &inFlightFence );

		Sleep( 1 );
	}

	void record ( VkCommandBuffer buffer, uint imageIndex ) {
		VkCommandBufferBeginInfo info = new() {
			sType = VkStructureType.CommandBufferBeginInfo
		};

		VulkanExtensions.Validate( Vk.vkBeginCommandBuffer( buffer, &info ) );

		VkClearValue clearValue = this.clearValue;
		VkRenderPassBeginInfo beginInfo = new() {
			sType = VkStructureType.RenderPassBeginInfo,
			renderPass = renderPass,
			framebuffer = framebuffers[imageIndex],
			renderArea = { 
				offset = { x = 0, y = 0 }, 
				extent = swapchainSize
			},
			clearValueCount = 1,
			pClearValues = &clearValue
		};

		Vk.vkCmdBeginRenderPass( buffer, &beginInfo, VkSubpassContents.Inline );
		Vk.vkCmdBindPipeline( buffer, VkPipelineBindPoint.Graphics, pipeline );

		VkViewport viewport = new() {
			x = 0,
			y = 0,
			width = swapchainSize.width,
			height = swapchainSize.height,
			minDepth = 0,
			maxDepth = 1
		};

		VkRect2D scissor = new() {
			offset = new() { x = 0, y = 0 },
			extent = swapchainSize
		};

		Vk.vkCmdSetViewport( buffer, 0, 1, &viewport );
		Vk.vkCmdSetScissor( buffer, 0, 1, &scissor );

		Vk.vkCmdDraw( buffer, 3, 1, 0, 0 );
		Vk.vkCmdEndRenderPass( buffer );

		VulkanExtensions.Validate( Vk.vkEndCommandBuffer( buffer ) );
	}

	protected override void Dispose ( bool disposing ) {
		Vk.vkDestroySemaphore( device, imageAvailableSemaphore, vulkan.Allocator );
		Vk.vkDestroySemaphore( device, renderFinishedSemaphore, vulkan.Allocator );
		Vk.vkDestroyFence( device, inFlightFence, vulkan.Allocator );
		Vk.vkDestroyCommandPool( device, commnadPool, vulkan.Allocator );
		foreach ( var i in framebuffers )
			Vk.vkDestroyFramebuffer( device, i, vulkan.Allocator );
		Vk.vkDestroyPipeline( device, pipeline, vulkan.Allocator );
		Vk.vkDestroyPipelineLayout( device, pipelineLayout, vulkan.Allocator );
		Vk.vkDestroyRenderPass( device, renderPass, vulkan.Allocator );
		foreach ( var i in imageViews )
			Vk.vkDestroyImageView( device, i, vulkan.Allocator );
		Vk.vkDestroySwapchainKHR( device, swapchain, vulkan.Allocator );
		Vk.vkDestroyDevice( device, vulkan.Allocator );
		Vk.vkDestroySurfaceKHR( vulkan.Instance, surface, vulkan.Allocator );
		vulkan.Dispose();
	}
}
