using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Vulkan.Shaders;
using Vit.Framework.Graphics.Vulkan.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vulkan;
using Buffer = Vit.Framework.Graphics.Vulkan.Buffers.Buffer;

namespace Vit.Framework.Graphics.Vulkan.Rendering;

public class VulkanDeferredCommandBuffer : BasicCommandBuffer<VulkanRenderer, FrameBuffer, IVulkanTexture, ShaderSet>, IDeferredCommandBuffer {
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

	protected override void RenderTo ( FrameBuffer framebuffer ) {
		Buffer.BeginRenderPass( framebuffer );
	}

	protected override void FinishRendering () {
		Buffer.FinishRenderPass();
	}

	public unsafe override void ClearColor<T> ( T _color ) {
		var span = _color.AsSpan();
		var value = new VkClearValue() {
			color = new(
				span.Length >= 1 ? span[0] : 0,
				span.Length >= 2 ? span[1] : 0,
				span.Length >= 3 ? span[2] : 0,
				span.Length >= 4 ? span[3] : 1
			)
		};

		VkClearAttachment attachment = new() {
			aspectMask = VkImageAspectFlags.Color,
			clearValue = value,
			colorAttachment = 0
		};
		VkClearRect rect = new() {
			baseArrayLayer = 0,
			layerCount = 1,
			rect = { extent = Framebuffer!.Size }
		};
		Vk.vkCmdClearAttachments( Buffer, 1, &attachment, 1, &rect );
	}

	public unsafe override void ClearDepth ( float depth ) {
		var value = new VkClearValue() {
			depthStencil = new( depth, 0 )
		};

		VkClearAttachment attachment = new() {
			aspectMask = VkImageAspectFlags.Depth,
			clearValue = value
		};
		VkClearRect rect = new() {
			baseArrayLayer = 0,
			layerCount = 1,
			rect = { extent = Framebuffer!.Size }
		};
		Vk.vkCmdClearAttachments( Buffer, 1, &attachment, 1, &rect );
	}

	public unsafe override void ClearStencil ( uint stencil ) {
		var value = new VkClearValue() {
			depthStencil = new( 0, stencil )
		};

		VkClearAttachment attachment = new() {
			aspectMask = VkImageAspectFlags.Stencil,
			clearValue = value
		};
		VkClearRect rect = new() {
			baseArrayLayer = 0,
			layerCount = 1,
			rect = { extent = Framebuffer!.Size }
		};
		Vk.vkCmdClearAttachments( Buffer, 1, &attachment, 1, &rect );
	}

	protected override unsafe void CopyTexture ( IVulkanTexture source, IVulkanTexture destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset ) {
		Debug.Assert( source.Format == PixelFormat.Rgba8 );
		Debug.Assert( destination.Format == PixelFormat.Rgba8 );

		switch ((source.Type, destination.Type)) {
			case (VulkanTextureType.Buffer, VulkanTextureType.Image):
				var src = (StagingImage)source;
				var dst = (Image)destination;

				dst.TransitionLayout( VkImageLayout.TransferDstOptimal, VkImageAspectFlags.Color, Buffer ); // TODO also bad! (aspect)
				var region = new VkBufferImageCopy() {
					bufferOffset = sizeof(byte) * 4 * (sourceRect.MinX + sourceRect.MinY * source.Size.Width),
					bufferRowLength = source.Size.Width,
					bufferImageHeight = source.Size.Height,
					imageSubresource = {
						aspectMask = VkImageAspectFlags.Color,
						mipLevel = 0,
						baseArrayLayer = 0,
						layerCount = 1
					},
					imageOffset = {
						x = (int)destinationOffset.X,
						y = (int)destinationOffset.Y
					},
					imageExtent = {
						width = sourceRect.Width,
						height = sourceRect.Height,
						depth = 1
					}
				};

				Vk.vkCmdCopyBufferToImage( Buffer, src, dst, VkImageLayout.TransferDstOptimal, 1, &region );
				dst.TransitionLayout( VkImageLayout.ShaderReadOnlyOptimal, VkImageAspectFlags.Color, Buffer );
				break;

			default:
				throw new NotImplementedException();
		}
	}

	public override void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var src = (IVulkanHandle<VkBuffer>)source;
		var dst = (IVulkanHandle<VkBuffer>)destination;

		Buffer.Copy( src.Handle, dst.Handle, length, sourceOffset, destinationOffset );
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
				RenderPass = Framebuffer!.RenderPass,
				DepthTest = DepthTest.IsEnabled ? DepthTest : new BufferTest() { IsEnabled = false },
				DepthState = DepthTest.IsEnabled ? DepthState : new DepthState { WriteOnPass = false },
				StencilTest = StencilTest.IsEnabled ? StencilTest : new BufferTest() { IsEnabled = false },
				StencilState = StencilTest.IsEnabled ? StencilState : new StencilState( StencilOperation.Keep ),
				BlendState = BlendState.IsEnabled ? BlendState : new BlendState { IsEnabled = false }
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

	protected override void UpdateUniforms () {
		if ( ShaderSet.DescriptorSets.Length != 0 )
			Buffer.BindDescriptors( pipeline.Layout, ShaderSet.DescriptorSets );
	}

	VkBuffer[] vertexBuffers = new VkBuffer[16];
	ulong[] bufferOffsets = new ulong[16];
	protected override bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset ) {
		var offsetSet = ((ulong)offset).TrySet( ref bufferOffsets[binding] );

		var buf = ((IVulkanHandle<VkBuffer>)buffer).Handle;
		if ( vertexBuffers[binding] != buf ) {
			vertexBuffers[binding] = buf;
			return true;
		}

		return offsetSet;
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			Buffer.BindVertexBuffers( vertexBuffers.AsSpan( 0, ShaderSet.VertexBufferCount ) );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			if ( IndexBufferType == IndexBufferType.UInt16 )
				Buffer.BindU16IndexBuffer( (Buffer)IndexBuffer );
			else
				Buffer.BindU32IndexBuffer( (Buffer)IndexBuffer );
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Buffer.DrawIndexed( vertexCount, offset );
	}
}