﻿using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Rendering;

public class GlImmediateCommandBuffer : IImmediateCommandBuffer {
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		GL.BindFramebuffer( FramebufferTarget.Framebuffer, ((IGlFramebuffer)framebuffer).Handle );

		var color = clearColor ?? new( 0, 0, 0, 1 );
		GL.ClearColor( color.R, color.G, color.B, color.A );
		GL.ClearStencil( (int)(clearStencil ?? 0) );
		GL.ClearDepth( clearDepth ?? 0 );
		GL.Clear( ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.ColorBufferBit );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			GL.BindFramebuffer( FramebufferTarget.Framebuffer, 0 );
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		throw new NotImplementedException();
	}

	public void SetShaders ( IShaderSet? shaders ) {
		throw new NotImplementedException();
	}

	public void SetTopology ( Topology topology ) {
		throw new NotImplementedException();
	}

	public void SetViewport ( AxisAlignedBox2<float> viewport ) {
		throw new NotImplementedException();
	}

	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		throw new NotImplementedException();
	}

	public void BindVertexBuffer ( IBuffer buffer ) {
		throw new NotImplementedException();
	}

	public void BindIndexBuffer ( IBuffer buffer ) {
		throw new NotImplementedException();
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		throw new NotImplementedException();
	}

	public void Dispose () {
		
	}
}
