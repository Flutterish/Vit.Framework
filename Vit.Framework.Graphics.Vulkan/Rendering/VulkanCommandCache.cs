using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanCommandCache : ICommandCache {
	public readonly CommandBuffer Buffer;
	public readonly VulkanRenderer Renderer;

	public VulkanCommandCache ( CommandBuffer buffer, VulkanRenderer renderer ) {
		Buffer = buffer;
		Renderer = renderer;
	}

	public void Reset () {
		Buffer.Reset();
	}

	public DisposeAction<ICommandCache> Begin () {
		Buffer.Begin();
		return new DisposeAction<ICommandCache>( this, static self => {
			((VulkanCommandCache)self).Buffer.Finish();
		} );
	}

	FrameBuffer? frameBuffer;
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		VkClearColorValue color = clearColor is ColorRgba<float> v
			? new VkClearColorValue( v.R, v.G, v.B, v.A )
			: new VkClearColorValue( 0, 0, 0, 1 );
		VkClearDepthStencilValue depthStencil = new VkClearDepthStencilValue( clearDepth ?? 0, clearStencil ?? 0 );
		Buffer.BeginRenderPass( frameBuffer = (FrameBuffer)framebuffer, new VkClearValue { color = color }, new VkClearValue { depthStencil = depthStencil } );
		// TODO instead have separate clear commands

		return new DisposeAction<ICommandBuffer>( this, static self => {
			((VulkanCommandCache)self).Buffer.FinishRenderPass();
			((VulkanCommandCache)self).frameBuffer = null;
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		((DeviceBuffer<T>)buffer).Transfer( data, offset, Buffer );
	}

	public void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		((ImageTexture)texture).Image.Transfer( data, Buffer );
	}

	ShaderSet? shaders;
	Topology topology;
	AxisAlignedBox2<uint> viewport;
	AxisAlignedBox2<uint> scissors;
	IBuffer? indexBuffer;
	IBuffer? oldIndexBuffer;
	IBuffer? vertexBuffer;
	IBuffer? oldVertexBuffer;
	bool pipelineInvalidated = true;
	public void SetShaders ( IShaderSet? shaders ) {
		this.shaders = shaders as ShaderSet;
		pipelineInvalidated = true;
	}

	public void SetTopology ( Topology topology ) {
		this.topology = topology;
		pipelineInvalidated = true;
	}

	public void SetViewport ( AxisAlignedBox2<uint> viewport ) {
		this.viewport = viewport;
	}

	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		this.scissors = scissors;
	}

	public void BindVertexBuffer ( IBuffer buffer ) {
		vertexBuffer = buffer;
	}

	public void BindIndexBuffer ( IBuffer buffer ) {
		indexBuffer = buffer;
	}

	Pipeline? pipeline;
	void updatePipeline () {
		if ( !pipelineInvalidated ) {
			return;
		}

		pipelineInvalidated = false;

		if ( topology != Topology.Triangles || shaders == null || frameBuffer == null )
			throw new Exception( "you forgor" );

		var next = Renderer.GetPipeline( new() {
			Shaders = shaders,
			RenderPass = frameBuffer.RenderPass
		} );

		if ( next == pipeline ) {
			throw new Exception( "I cant do this anymore /j" );
		}
		else {
			pipeline = next;
			Buffer.BindPipeline( pipeline );

			if ( vertexBuffer != null )
				Buffer.BindVertexBuffer( (IVulkanHandle<VkBuffer>)vertexBuffer! );
			if ( indexBuffer != null ) {
				if ( indexBuffer is Buffer<ushort> shortBuffer )
					Buffer.BindIndexBuffer( shortBuffer );
				else
					Buffer.BindIndexBuffer( (Buffer<uint>)indexBuffer );
			}

			Buffer.SetViewPort( new() {
				minDepth = 0,
				maxDepth = 1,
				x = viewport.MinX,
				y = viewport.MinY,
				width = viewport.Width,
				height = viewport.Height
			} );
			Buffer.SetScissor( new() {
				offset = {
					x = (int)scissors.MinX,
					y = (int)scissors.MinY
				},
				extent = {
					width = scissors.Width,
					height = scissors.Height
				}
			} );

			Buffer.BindDescriptor( pipeline.Layout, shaders.DescriptorSet );
		}
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		updatePipeline();

		Debug.Assert( offset == 0 );
		Buffer.DrawIndexed( vertexCount ); // TODO offset
	}
}