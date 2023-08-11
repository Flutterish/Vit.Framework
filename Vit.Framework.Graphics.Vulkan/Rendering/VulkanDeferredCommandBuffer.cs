using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Vulkan.Buffers;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vulkan;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanDeferredCommandBuffer : BasicCommandBuffer<VulkanRenderer, FrameBuffer, ImageTexture, ShaderSet>, IDeferredCommandBuffer {
	public readonly CommandBuffer Buffer;
	public VulkanDeferredCommandBuffer ( CommandBuffer buffer, VulkanRenderer renderer ) : base( renderer ) {
		Buffer = buffer;
	}

	public DisposeAction<IDeferredCommandBuffer> Begin () {
		Buffer.Begin();
		return new DisposeAction<IDeferredCommandBuffer>( this, static self => {
			((VulkanDeferredCommandBuffer)self).Buffer.Finish();
		} );
	}

	public void Reset () {
		Buffer.Reset();
	}

	FrameBuffer frameBuffer = null!;
	protected override DisposeAction<ICommandBuffer> RenderTo ( FrameBuffer framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil ) {
		VkClearColorValue color = clearColor.BitCast<ColorRgba<float>, VkClearColorValue>();
		VkClearDepthStencilValue depthStencil = new VkClearDepthStencilValue( clearDepth, clearStencil );
		Buffer.BeginRenderPass( this.frameBuffer = framebuffer, new VkClearValue { color = color }, new VkClearValue { depthStencil = depthStencil } );
		// TODO instead have separate clear commands

		return new DisposeAction<ICommandBuffer>( this, static self => {
			((VulkanDeferredCommandBuffer)self).Buffer.FinishRenderPass();
			((VulkanDeferredCommandBuffer)self).frameBuffer = null!;
		} );
	}

	public override void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) {
		((DeviceBuffer<T>)buffer).Transfer( data, offset, Buffer );
	}

	protected override void UploadTextureData<TPixel> ( ImageTexture texture, ReadOnlySpan<TPixel> data ) {
		texture.Image.Transfer( data, Buffer );
	}

	Pipeline pipeline = null!;
	const PipelineInvalidations pipelineInvalidations = PipelineInvalidations.Shaders | PipelineInvalidations.Framebuffer | PipelineInvalidations.Topology 
		| PipelineInvalidations.DepthTest | PipelineInvalidations.StencilTest;
	const PipelineInvalidations dynamicState = PipelineInvalidations.Scissors | PipelineInvalidations.Viewport;
	protected override void UpdatePieline ( PipelineInvalidations invalidations ) {
		if ( (invalidations & pipelineInvalidations) != 0 ) {
			Debug.Assert( Topology == Topology.Triangles ); // TODO topology
			pipeline = Renderer.GetPipeline( new() { 
				Shaders = ShaderSet,
				RenderPass = frameBuffer.RenderPass,
				DepthTest = DepthTest.IsEnabled ? DepthTest : new BufferTest() { IsEnabled = false },
				DepthState = DepthTest.IsEnabled ? DepthState : new DepthState { WriteOnPass = false },
				StencilTest = StencilTest.IsEnabled ? StencilTest : new BufferTest() { IsEnabled = false },
				StencilState = StencilTest.IsEnabled ? StencilState : new StencilState( StencilOperation.Keep )
			} );

			Buffer.BindPipeline( pipeline );

			invalidations |= dynamicState;
		}

		if ( (invalidations & PipelineInvalidations.Viewport) != 0 ) {
			Buffer.SetViewPort( new() {
				minDepth = 0,
				maxDepth = 1,
				x = Viewport.MinX,
				y = Viewport.MinY,
				width = Viewport.Width,
				height = Viewport.Height
			} );
		}

		if ( (invalidations & PipelineInvalidations.Scissors) != 0 ) {
			Buffer.SetScissor( new() {
				offset = {
					x = (int)Scissors.MinX,
					y = (int)Scissors.MinY
				},
				extent = {
					width = Scissors.Width,
					height = Scissors.Height
				}
			} );
		}
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( ShaderSet.DescriptorSets.Length != 0 )
			Buffer.BindDescriptors( pipeline.Layout, ShaderSet.DescriptorSets );

		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) )
			Buffer.BindVertexBuffer( (IVulkanHandle<VkBuffer>)VertexBuffer );

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			if ( IndexBufferType == IndexBufferType.UInt16 )
				Buffer.BindIndexBuffer( (Buffer<ushort>)IndexBuffer );
			else
				Buffer.BindIndexBuffer( (Buffer<uint>)IndexBuffer );
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Debug.Assert( offset == 0 );
		Buffer.DrawIndexed( vertexCount ); // TODO offset
	}
}