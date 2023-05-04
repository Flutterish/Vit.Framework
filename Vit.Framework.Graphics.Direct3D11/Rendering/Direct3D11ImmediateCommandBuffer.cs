using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Direct3D11.Shaders;
using Vit.Framework.Graphics.Direct3D11.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Rendering;

public class Direct3D11ImmediateCommandBuffer : DisposableObject, IImmediateCommandBuffer {
	public readonly ID3D11DeviceContext Context;
	public Direct3D11ImmediateCommandBuffer ( ID3D11DeviceContext context ) {
		Context = context;
	}

	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		Context.OMSetRenderTargets( ((TargetView)framebuffer).Handle );
		var color = clearColor ?? ColorRgba.Black;
		Context.ClearRenderTargetView( ((TargetView)framebuffer).Handle, new( color.R, color.G, color.B, color.A ) );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			// TODO set to null?
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		((Buffer<T>)buffer).Upload( data, offset );
	}

	public void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		((Texture2D)texture).Upload( data, Context );
	}

	ShaderSet? shaders;
	public void SetShaders ( IShaderSet? shaders ) {
		if ( shaders is null )
			return;

		this.shaders = (ShaderSet)shaders;
		foreach ( var i in ((ShaderSet)shaders).Shaders ) {
			i.Bind( Context );
		}
		Context.IASetInputLayout( ((ShaderSet)shaders).Layout! );
	}

	public void SetTopology ( Topology topology ) {
		Context.IASetPrimitiveTopology( topology switch {
			Topology.Triangles => PrimitiveTopology.TriangleList,
			_ => throw new ArgumentException( "Invalid topology", nameof(topology) )
		} );
	}

	public void SetViewport ( AxisAlignedBox2<uint> viewport ) {
		Context.RSSetViewport( viewport.MinX, viewport.MinY, viewport.Width, viewport.Height );
	}

	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		// TODO scissors
	}

	public void BindVertexBuffer ( IBuffer buffer ) {
		Context.IASetVertexBuffer( 0, ( (ID3D11BufferHandle)buffer).Handle!, shaders!.Stride );
	}

	public void BindIndexBuffer ( IBuffer buffer ) {
		Context.IASetIndexBuffer( ((ID3D11BufferHandle)buffer).Handle!, buffer is Buffer<uint> ? Vortice.DXGI.Format.R32_UInt : Vortice.DXGI.Format.R16_UInt, 0 );
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		Context.DrawIndexed( (int)vertexCount, (int)offset, 0 );
	}

	protected override void Dispose ( bool disposing ) {
		
	}
}
