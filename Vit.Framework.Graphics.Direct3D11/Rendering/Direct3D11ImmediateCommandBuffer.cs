using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Mathematics;

namespace Vit.Framework.Graphics.Direct3D11.Rendering;

public class Direct3D11ImmediateCommandBuffer : BasicCommandBuffer<Direct3D11Renderer, TargetView, Texture2D, ShaderSet>, IImmediateCommandBuffer {
	public readonly ID3D11DeviceContext Context;
	public Direct3D11ImmediateCommandBuffer ( Direct3D11Renderer renderer, ID3D11DeviceContext context ) : base( renderer ) {
		Context = context;
		Renderer.ActiveImmediateContextState?.InvalidateAll();
		Renderer.ActiveImmediateContextState = this;
	}

	ID3D11RenderTargetView[]? previousFrameBufferAttachments;
	ID3D11DepthStencilView? previousFrameBufferDepthStencil;
	protected override void RenderTo ( TargetView framebuffer ) {
		previousFrameBufferAttachments = Renderer.ActiveImmediateContextFrameBufferAttachments;
		previousFrameBufferDepthStencil = Renderer.ActiveImmediateContextFrameBufferDepthStencil;

		Context.OMSetRenderTargets( 
			Renderer.ActiveImmediateContextFrameBufferAttachments = framebuffer.ColorAttachments, 
			Renderer.ActiveImmediateContextFrameBufferDepthStencil = framebuffer.DepthStencil
		);
	}
	protected override void FinishRendering () {
		if ( previousFrameBufferAttachments == null )
			return;

		Context.OMSetRenderTargets(
			Renderer.ActiveImmediateContextFrameBufferAttachments = previousFrameBufferAttachments,
			Renderer.ActiveImmediateContextFrameBufferDepthStencil = previousFrameBufferDepthStencil
		);
	}

	public override void ClearColor<T> ( T color ) {
		var span = color.AsSpan();
		Color4 _color = new(
			red: span.Length >= 1 ? span[0] : 0,
			green: span.Length >= 2 ? span[1] : 0,
			blue: span.Length >= 3 ? span[2] : 0,
			alpha: span.Length >= 4 ? span[3] : 1
		);

		foreach ( var i in Framebuffer!.ColorAttachments ) {
			Context.ClearRenderTargetView( i, _color );
		}
	}

	public override void ClearDepth ( float depth ) {
		if ( Framebuffer!.DepthStencil is not ID3D11DepthStencilView depthStencil )
			return;

		Context.ClearDepthStencilView( depthStencil, DepthStencilClearFlags.Depth, depth, 0 );
	}

	public override void ClearStencil ( uint stencil ) {
		if ( Framebuffer!.DepthStencil is not ID3D11DepthStencilView depthStencil )
			return;

		Context.ClearDepthStencilView( depthStencil, DepthStencilClearFlags.Stencil, 0, (byte)stencil );
	}

	protected override void CopyTexture ( Texture2D source, Texture2D destination, AxisAlignedBox2<uint> sourceRect, Point2<uint> destinationOffset ) {
		Context.CopySubresourceRegion( destination.Texture, 0, (int)destinationOffset.X, (int)destinationOffset.Y, 0, source.Texture, 0, new() {
			Left = (int)sourceRect.MinX,
			Right = (int)sourceRect.MaxX,
			Top = (int)sourceRect.MinY,
			Bottom = (int)sourceRect.MaxY,
			Back = 1
		} );
	}

	public override void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var src = (ID3D11BufferHandle)source;
		var dst = (ID3D11BufferHandle)destination;

		Context.CopySubresourceRegion( dst.Handle, 0, (int)destinationOffset, 0, 0, src.Handle, 0, new() {
			Left = (int)sourceOffset,
			Right = (int)(sourceOffset + length),
			Bottom = 1,
			Back = 1
		} );
	}

	protected override void UpdatePieline ( PipelineInvalidations invalidations ) {
		if ( invalidations.HasFlag( PipelineInvalidations.Shaders ) ) {
			foreach ( var i in ( ShaderSet.LinkedShaders ) ) {
				i.Bind( Context );
			}

			Context.IASetInputLayout( ShaderSet.Layout );
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Topology ) ) {
			Context.IASetPrimitiveTopology( Topology switch {
				Topology.Triangles => PrimitiveTopology.TriangleList,
				_ => throw new ArgumentException( "Invalid topology", nameof( Topology ) )
			} );
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Viewport ) ) {
			Context.RSSetViewport( Viewport.MinX, Viewport.MinY, Viewport.Width, Viewport.Height );
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Scissors ) ) {
			// TODO scissors
		}

		if ( (invalidations & (PipelineInvalidations.DepthTest | PipelineInvalidations.StencilTest)) != 0 ) {
			Context.OMSetDepthStencilState( Renderer.GetDepthStencilState( new() {
				DepthTest = DepthTest.IsEnabled ? DepthTest : new() { IsEnabled = false },
				DepthState = DepthTest.IsEnabled ? DepthState : new() { WriteOnPass = false },
				StencilTest = StencilTest.IsEnabled ? StencilTest : new() { IsEnabled = false },
				StencilState = StencilTest.IsEnabled ? StencilState : new StencilState( Graphics.Rendering.StencilOperation.Keep )
			} ), stencilRef: StencilState.ReferenceValue.BitCast<uint, int>() );
		}

		if ( invalidations.HasFlag( PipelineInvalidations.Blending ) ) {
			Context.OMSetBlendState( Renderer.GetBlendState( BlendState ), new Color4( BlendState.Constant.X, BlendState.Constant.Y, BlendState.Constant.Z, BlendState.Constant.W ) );
		}
	}

	protected override void UpdateUniforms () {
		foreach ( var i in ShaderSet.UniformSets ) {
			i.Apply( Context );
		}
	}

	ID3D11BufferHandle[] vertexBuffers = new ID3D11BufferHandle[16];
	ID3D11Buffer[] buffers = new ID3D11Buffer[16];
	int[] bufferOffsets = new int[16];
	protected override bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset ) {
		var bufferSet = ((ID3D11BufferHandle)buffer).TrySet( ref vertexBuffers[binding] );
		var offsetSet = ((int)offset).TrySet( ref bufferOffsets[binding] );

		return bufferSet || offsetSet;
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			Context.IASetIndexBuffer( ((ID3D11BufferHandle)IndexBuffer).Handle!, IndexBufferType == IndexBufferType.UInt32 ? Vortice.DXGI.Format.R32_UInt : Vortice.DXGI.Format.R16_UInt, (int)IndexBufferOffset );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			for ( int i = 0; i < ShaderSet.BufferStrides.Length; i++ ) { // TODO this could be avoided if we could reallocate without changing the handle like opengl and vulkan
				buffers[i] = vertexBuffers[i].Handle!;
			}
			Context.IASetVertexBuffers( 0, ShaderSet.BufferStrides.Length, buffers, ShaderSet.BufferStrides, bufferOffsets );
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Context.DrawIndexed( (int)vertexCount, (int)offset, 0 );
	}

	public void Dispose () { }
}