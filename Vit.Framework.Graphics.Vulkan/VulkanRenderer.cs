using System.Numerics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan;

public class VulkanRenderer : DisposableObject, IRenderer {
	public GraphicsApi GraphicsApi { get; }

	public readonly Device Device;
	public readonly Queue GraphicsQueue;
	public readonly CommandPool GraphicsCommandPool;
	public readonly CommandPool CopyCommandPool;
	public readonly CommandBuffer GraphicsCommands;

	public VulkanRenderer ( VulkanApi api, Device device, Queue graphicsQueue ) {
		GraphicsApi = api;
		Device = device;
		GraphicsQueue = graphicsQueue;
		GraphicsCommandPool = Device.CreateCommandPool( graphicsQueue );
		CopyCommandPool = Device.CreateCommandPool( graphicsQueue, VkCommandPoolCreateFlags.ResetCommandBuffer | VkCommandPoolCreateFlags.Transient );

		GraphicsCommands = GraphicsCommandPool.CreateCommandBuffer();
	}

	public void WaitIdle () {
		Device.WaitIdle();
	}

	public Matrix4<T> CreateNdcCorrectionMatrix<T> () where T : INumber<T> {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}
	public Matrix4<T> CreateUvCorrectionMatrix<T> () where T : INumber<T> {
		return Matrix4<T>.Identity;
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return Device.CreateShaderModule( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts, VertexInputDescription? vertexInput ) {
		return new ShaderSet( parts.Select( x => (ShaderModule)x ), vertexInput );
	}

	public IHostBuffer<T> CreateHostBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return new HostBuffer<T>( Device, size, type switch {
			BufferType.Vertex => VkBufferUsageFlags.VertexBuffer,
			BufferType.Index => VkBufferUsageFlags.IndexBuffer,
			BufferType.Uniform => VkBufferUsageFlags.UniformBuffer,
			BufferType.ReadonlyStorage => VkBufferUsageFlags.StorageBuffer,
			_ => throw new ArgumentException( "Buffer type not supported", nameof( type ) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBufferRaw<T> ( uint size, BufferType type, BufferUsage usage ) where T : unmanaged {
		return new DeviceBuffer<T>( Device, size, type switch {
			BufferType.Vertex => VkBufferUsageFlags.VertexBuffer,
			BufferType.Index => VkBufferUsageFlags.IndexBuffer,
			BufferType.Uniform => VkBufferUsageFlags.UniformBuffer,
			BufferType.ReadonlyStorage => VkBufferUsageFlags.StorageBuffer,
			_ => throw new ArgumentException( "Buffer type not supported", nameof( type ) )
		} );
	}

	public IStagingBuffer<T> CreateStagingBufferRaw<T> ( uint size, BufferUsage usage ) where T : unmanaged {
		return new HostBuffer<T>( Device, size, VkBufferUsageFlags.TransferSrc );
	}

	public IDeviceTexture2D CreateDeviceTexture ( Size2<uint> size, PixelFormat format ) {
		return new Image( Device, size, format );
	}

	public IStagingTexture2D CreateStagingTexture ( Size2<uint> size, PixelFormat format ) {
		return new StagingImage( Device, size, format );
	}

	public ISampler CreateSampler ( SamplerDescription description ) {
		return new Sampler( Device, description );
	}

	public unsafe IFramebuffer CreateFramebuffer ( IEnumerable<IDeviceTexture2D> attachments, IDeviceTexture2D? depthStencilAttachment = null ) {
		var renderPass = new RenderPass( Device, attachments, depthStencilAttachment );
		var imageViews = new VkImageView[attachments.Count() + (depthStencilAttachment == null ? 0 : 1)];
		uint i = 0;
		foreach ( var color in attachments ) {
			var info = new VkImageViewCreateInfo() {
				sType = VkStructureType.ImageViewCreateInfo,
				image = (Image)color,
				viewType = VkImageViewType.Image2D,
				format = VulkanApi.formats[color.Format],
				subresourceRange = {
					aspectMask = VkImageAspectFlags.Color,
					baseMipLevel = 0,
					levelCount = 1,
					baseArrayLayer = 0,
					layerCount = 1
				}
			};
			Vk.vkCreateImageView( Device, &info, VulkanExtensions.TODO_Allocator, out imageViews[i] );
			i++;
		}
		if ( depthStencilAttachment != null ) {
			var info = new VkImageViewCreateInfo() {
				sType = VkStructureType.ImageViewCreateInfo,
				image = (Image)depthStencilAttachment,
				viewType = VkImageViewType.Image2D,
				format = VulkanApi.formats[depthStencilAttachment.Format],
				subresourceRange = {
					aspectMask = depthStencilAttachment.Format.Type switch {
						PixelType.Stencil => VkImageAspectFlags.Stencil,
						PixelType.Depth => VkImageAspectFlags.Depth,
						_ => VkImageAspectFlags.Stencil | VkImageAspectFlags.Depth
					},
					baseMipLevel = 0,
					levelCount = 1,
					baseArrayLayer = 0,
					layerCount = 1
				}
			};
			Vk.vkCreateImageView( Device, &info, VulkanExtensions.TODO_Allocator, out imageViews[i] );
		}

		var single = depthStencilAttachment ?? attachments.Single();
		return new FrameBuffer( imageViews, new VkExtent2D { width = single.Size.Width, height = single.Size.Height }, renderPass, isOwner: true );
	}

	public IImmediateCommandBuffer CreateImmediateCommandBuffer () {
		var commands = GraphicsCommandPool.CreateCommandBuffer();
		commands.Begin();
		return new VulkanImmediateCommandBuffer( commands, this, commands => {
			commands.Buffer.Finish();
			commands.Buffer.Submit( GraphicsQueue );
			WaitIdle(); // TODO bad!
			GraphicsCommandPool.FreeCommandBuffer( commands.Buffer );
		} );
	}

	Dictionary<PipelineArgs, Pipeline> pipelines = new(); // TODO move this into render pass?
	public Pipeline GetPipeline ( PipelineArgs args ) {
		if ( pipelines.TryGetValue( args, out var pipeline ) )
			return pipeline;

		pipeline = new Pipeline( Device, args );
		pipelines.Add( args, pipeline );
		return pipeline;
	}

	IRendererSpecialisation IRenderer.Specialisation => Specialisation;
	public static readonly VulkanRendererSpecialisation Specialisation = default;

	protected override void Dispose ( bool disposing ) {
		foreach ( var (_, pipeline) in pipelines ) {
			pipeline.Dispose();
		}

		GraphicsCommandPool.Dispose();
		CopyCommandPool.Dispose();
		Device.Dispose();
	}
}

public struct PipelineArgs {
	public required ShaderSet Shaders;
	public required RenderPass RenderPass;
	public required BufferTest DepthTest;
	public required DepthState DepthState;
	public required BufferTest StencilTest;
	public required StencilState StencilState;
	public required BlendState BlendState;
}