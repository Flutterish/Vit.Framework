﻿using Vit.Framework.Graphics.Direct3D11.Buffers;
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

	protected override DisposeAction<ICommandBuffer> RenderTo ( TargetView framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil ) {
		Context.OMSetRenderTargets( framebuffer.Handle, framebuffer.DepthStencil );
		Context.ClearRenderTargetView( framebuffer.Handle, new( clearColor.R, clearColor.G, clearColor.B, clearColor.A ) );
		if ( framebuffer.DepthStencil is ID3D11DepthStencilView depthStencil )
			Context.ClearDepthStencilView( depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, clearDepth, (byte)clearStencil );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			// TODO set to null?
		} );
	}

	public override void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) {
		((DeviceBuffer<T>)buffer).Upload( data, offset, Context );
	}

	protected override void UploadTextureData<TPixel> ( Texture2D texture, ReadOnlySpan<TPixel> data ) {
		texture.Upload( data, Context );
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

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {
		foreach ( var (index, set) in ShaderSet.UniformSets ) {
			set.Apply( index, ShaderSet, Context );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Index ) ) {
			Context.IASetIndexBuffer( ((ID3D11BufferHandle)IndexBuffer).Handle!, IndexBufferType == IndexBufferType.UInt32 ? Vortice.DXGI.Format.R32_UInt : Vortice.DXGI.Format.R16_UInt, 0 );
		}

		if ( invalidations.HasFlag( BufferInvalidations.Vertex ) ) {
			Context.IASetVertexBuffer( 0, ((ID3D11BufferHandle)VertexBuffer).Handle!, ShaderSet.Stride );
		}
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Context.DrawIndexed( (int)vertexCount, (int)offset, 0 );
	}

	public void Dispose () { }
}