﻿using System.Numerics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Queues;
using Vit.Framework.Graphics.Vulkan.Rendering;
using Vit.Framework.Graphics.Vulkan.Shaders;
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

	public Matrix4<T> CreateLeftHandCorrectionMatrix<T> () where T : INumber<T> {
		return new Matrix4<T> {
			M00 = T.MultiplicativeIdentity,
			M11 = -T.MultiplicativeIdentity,
			M22 = T.MultiplicativeIdentity,
			M33 = T.MultiplicativeIdentity
		};
	}

	public IShaderPart CompileShaderPart ( SpirvBytecode spirv ) {
		return Device.CreateShaderModule( spirv );
	}

	public IShaderSet CreateShaderSet ( IEnumerable<IShaderPart> parts ) {
		return new ShaderSet( parts.Select( x => (ShaderModule)x ) );
	}

	public IHostBuffer<T> CreateHostBuffer<T> ( BufferType type ) where T : unmanaged {
		return new HostBuffer<T>( Device, type switch {
			BufferType.Vertex => VkBufferUsageFlags.VertexBuffer,
			BufferType.Index => VkBufferUsageFlags.IndexBuffer,
			BufferType.Uniform => VkBufferUsageFlags.UniformBuffer,
			_ => throw new ArgumentException( "Buffer type not supported", nameof(type) )
		} );
	}

	public IDeviceBuffer<T> CreateDeviceBuffer<T> ( BufferType type ) where T : unmanaged {
		return new DeviceBuffer<T>( Device, type switch {
			BufferType.Vertex => VkBufferUsageFlags.VertexBuffer,
			BufferType.Index => VkBufferUsageFlags.IndexBuffer,
			BufferType.Uniform => VkBufferUsageFlags.UniformBuffer,
			_ => throw new ArgumentException( "Buffer type not supported", nameof( type ) )
		} );
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

	Dictionary<PipelineArgs, Pipeline> pipelines = new();
	public Pipeline GetPipeline ( PipelineArgs args ) {
		if ( pipelines.TryGetValue( args, out var pipeline ) )
			return pipeline;

		pipeline = new Pipeline( Device, args.Shaders, args.RenderPass );
		pipelines.Add( args, pipeline );
		return pipeline;
	}

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
}