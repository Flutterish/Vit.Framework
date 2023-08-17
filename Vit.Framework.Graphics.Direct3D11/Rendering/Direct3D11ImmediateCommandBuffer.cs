using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Rendering;

public class Direct3D11ImmediateCommandBuffer : BasicCommandBuffer<Direct3D11Renderer, TargetView, Texture2D, ShaderSet>, IImmediateCommandBuffer {
	public readonly ID3D11DeviceContext Context;
	public Direct3D11ImmediateCommandBuffer ( Direct3D11Renderer renderer, ID3D11DeviceContext context ) : base( renderer ) {
		Context = context;
	}

	protected override DisposeAction<ICommandBuffer> RenderTo ( TargetView framebuffer, ColorSRgba<float> clearColor, float clearDepth, uint clearStencil ) {
		Context.OMSetRenderTargets( framebuffer.Handle, framebuffer.DepthStencil );
		Context.ClearRenderTargetView( framebuffer.Handle, new( clearColor.R, clearColor.G, clearColor.B, clearColor.A ) );
		if ( framebuffer.DepthStencil is ID3D11DepthStencilView depthStencil )
			Context.ClearDepthStencilView( depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, clearDepth, (byte)clearStencil );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			// TODO set to null?
		} );
	}

	protected override void UploadTextureData<TPixel> ( Texture2D texture, ReadOnlySpan<TPixel> data ) {
		texture.Upload( data, Context );
	}

	public override void CopyBufferRaw ( IBuffer source, IBuffer destination, uint length, uint sourceOffset = 0, uint destinationOffset = 0 ) {
		var src = (ID3D11BufferHandle)source;
		var dst = (ID3D11BufferHandle)destination;

		Context.CopySubresourceRegion( dst.Handle, 0, (int)destinationOffset, 0, 0, src.Handle, 0, new() {
			Left = (int)sourceOffset,
			Right = (int)(sourceOffset + length),
			Back = 1,
			Bottom = 1
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
		foreach ( var i in ShaderSet.UniformSets ) {
			i.Apply( Context );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			Context.IASetIndexBuffer( ((ID3D11BufferHandle)IndexBuffer).Handle!, IndexBufferType == IndexBufferType.UInt32 ? Vortice.DXGI.Format.R32_UInt : Vortice.DXGI.Format.R16_UInt, 0 );
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