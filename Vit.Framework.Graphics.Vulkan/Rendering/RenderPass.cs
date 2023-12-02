using Vit.Framework.Graphics.Rendering.Textures;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class RenderPass : DisposableVulkanObject<VkRenderPass> {
	public readonly Device Device;
	public readonly VkFormat ColorFormat;
	public readonly VkFormat AttachmentFormat; // TODO get rid of this. this is only used by the swapchain
	public readonly VkSampleCountFlags Samples;
	public unsafe RenderPass ( Device device, VkSampleCountFlags samples, VkFormat colorFormat, VkFormat attachmentFormat ) {
		ColorFormat = colorFormat;
		AttachmentFormat = attachmentFormat;
		Samples = samples;
		Device = device;

		var color = new VkAttachmentDescription() {
			format = colorFormat,
			samples = samples,
			loadOp = VkAttachmentLoadOp.DontCare,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.ColorAttachmentOptimal
		};
		var colorRef = new VkAttachmentReference() {
			attachment = 0,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		var depth = new VkAttachmentDescription() {
			format = attachmentFormat,
			samples = samples,
			loadOp = VkAttachmentLoadOp.DontCare,
			storeOp = VkAttachmentStoreOp.DontCare,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
		};
		var depthRef = new VkAttachmentReference() {
			attachment = 1,
			layout = VkImageLayout.DepthStencilAttachmentOptimal
		};

		var resolve = new VkAttachmentDescription() {
			format = colorFormat,
			samples = VkSampleCountFlags.Count1,
			loadOp = VkAttachmentLoadOp.DontCare,
			storeOp = VkAttachmentStoreOp.Store,
			stencilLoadOp = VkAttachmentLoadOp.DontCare,
			stencilStoreOp = VkAttachmentStoreOp.DontCare,
			initialLayout = VkImageLayout.Undefined,
			finalLayout = VkImageLayout.PresentSrcKHR
		};
		var resolveRef = new VkAttachmentReference() {
			attachment = 2,
			layout = VkImageLayout.ColorAttachmentOptimal
		};

		var attachments = new[] { color, depth, resolve };
		var subpass = new VkSubpassDescription() {
			pipelineBindPoint = VkPipelineBindPoint.Graphics,
			colorAttachmentCount = 1,
			pColorAttachments = &colorRef,
			pDepthStencilAttachment = &depthRef,
			pResolveAttachments = &resolveRef
		};

		var dependency = new VkSubpassDependency() {
			srcSubpass = ~0u,
			dstSubpass = 0,
			srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
			srcAccessMask = 0,
			dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
			dstAccessMask = VkAccessFlags.ColorAttachmentWrite | VkAccessFlags.DepthStencilAttachmentWrite
		};

		fixed ( VkAttachmentDescription* attachmentsPtr = attachments ) {
			var info = new VkRenderPassCreateInfo() {
				sType = VkStructureType.RenderPassCreateInfo,
				attachmentCount = (uint)attachments.Length,
				pAttachments = attachmentsPtr,
				subpassCount = 1,
				pSubpasses = &subpass,
				dependencyCount = 1,
				pDependencies = &dependency
			};

			Vk.vkCreateRenderPass( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
		}
	}

	public unsafe RenderPass ( Device device, IEnumerable<IDeviceTexture2D> _attachments, IDeviceTexture2D? depthStencilAttachment ) {
		Device = device;
		Samples = VkSampleCountFlags.Count1;

		var colorCount = _attachments.Count();
		var attachments = new VkAttachmentDescription[colorCount + (depthStencilAttachment == null ? 0 : 1)];
		var references = new VkAttachmentReference[attachments.Length];
		uint i = 0;
		foreach ( var color in _attachments ) {
			attachments[i] = new VkAttachmentDescription() {
				format = VulkanApi.formats[color.Format],
				samples = VkSampleCountFlags.Count1,
				loadOp = VkAttachmentLoadOp.DontCare,
				storeOp = VkAttachmentStoreOp.Store,
				stencilLoadOp = VkAttachmentLoadOp.DontCare,
				stencilStoreOp = VkAttachmentStoreOp.DontCare,
				initialLayout = VkImageLayout.Undefined,
				finalLayout = VkImageLayout.ShaderReadOnlyOptimal
			};
			references[i] = new VkAttachmentReference() {
				attachment = i,
				layout = VkImageLayout.ColorAttachmentOptimal
			};
			i++;
		}

		if ( depthStencilAttachment != null ) {
			attachments[i] = new VkAttachmentDescription() {
				format = VulkanApi.formats[depthStencilAttachment.Format],
				samples = VkSampleCountFlags.Count1,
				loadOp = VkAttachmentLoadOp.DontCare,
				storeOp = VkAttachmentStoreOp.DontCare,
				stencilLoadOp = VkAttachmentLoadOp.DontCare,
				stencilStoreOp = VkAttachmentStoreOp.DontCare,
				initialLayout = VkImageLayout.Undefined,
				finalLayout = VkImageLayout.DepthStencilAttachmentOptimal
			};
			references[i] = new VkAttachmentReference() {
				attachment = i,
				layout = VkImageLayout.DepthStencilAttachmentOptimal
			};
		}

		fixed ( VkAttachmentReference* referencesPtr = references ) {
			var subpass = new VkSubpassDescription() {
				pipelineBindPoint = VkPipelineBindPoint.Graphics,
				colorAttachmentCount = (uint)colorCount,
				pColorAttachments = referencesPtr,
				pDepthStencilAttachment = depthStencilAttachment == null ? null : (referencesPtr + references.Length - 1)
			};

			var dependency = new VkSubpassDependency() {
				srcSubpass = ~0u,
				dstSubpass = 0,
				srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
				srcAccessMask = 0,
				dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput | VkPipelineStageFlags.EarlyFragmentTests,
				dstAccessMask = VkAccessFlags.ColorAttachmentWrite | VkAccessFlags.DepthStencilAttachmentWrite
			};

			fixed ( VkAttachmentDescription* attachmentsPtr = attachments ) {
				var info = new VkRenderPassCreateInfo() {
					sType = VkStructureType.RenderPassCreateInfo,
					attachmentCount = (uint)attachments.Length,
					pAttachments = attachmentsPtr,
					subpassCount = 1,
					pSubpasses = &subpass,
					dependencyCount = 1,
					pDependencies = &dependency
				};

				Vk.vkCreateRenderPass( Device, &info, VulkanExtensions.TODO_Allocator, out Instance ).Validate();
			}
		}
	}

	Dictionary<PipelineArgs, Pipeline> pipelines = new();
	public Pipeline GetPipeline ( PipelineArgs args ) {
		if ( pipelines.TryGetValue( args, out var pipeline ) )
			return pipeline;

		pipeline = new Pipeline( Device, args );
		pipelines.Add( args, pipeline );
		return pipeline;
	}

	protected override unsafe void Dispose ( bool disposing ) {
		foreach ( var (_, pipeline) in pipelines ) {
			pipeline.Dispose();
		}

		Vk.vkDestroyRenderPass( Device, Instance, VulkanExtensions.TODO_Allocator );
	}
}
