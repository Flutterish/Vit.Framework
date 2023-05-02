using Vit.Framework.Graphics.OpenGl.Buffers;
using Vit.Framework.Graphics.OpenGl.Shaders;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;
using PrimitiveType = Vit.Framework.Graphics.Rendering.Shaders.Reflections.PrimitiveType;

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
		((Buffer<T>)buffer).Upload( data, offset );
	}

	IShaderSet? shaders;
	int vao;
	public void SetShaders ( IShaderSet? shaders ) {
		this.shaders = shaders;
		GL.UseProgram( ((ShaderProgram?)shaders)?.Handle ?? 0 );
	}

	public void SetTopology ( Topology topology ) {
		
	}

	public void SetViewport ( AxisAlignedBox2<uint> viewport ) {
		GL.Viewport( (int)viewport.MinX, (int)viewport.MinY, (int)viewport.Width, (int)viewport.Height );
	}

	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		GL.Scissor( (int)scissors.MinX, (int)scissors.MinY, (int)scissors.Width, (int)scissors.Height );
	}

	public void BindVertexBuffer ( IBuffer buffer ) {
		if ( vao == 0 ) {
			vao = GL.GenVertexArray();
			GL.BindVertexArray( vao );
		}

		GL.BindBuffer( BufferTarget.ArrayBuffer, ((IGlObject)buffer).Handle );
		int offset = 0;

		var stride = ((IGlBuffer)buffer).Stride;
		foreach ( var i in shaders!.Parts.First( x => x.Type == ShaderPartType.Vertex ).ShaderInfo.Input.Resources.OrderBy( x => x.Location ) ) {
			var length = (int)i.Type.FlattendedDimensions;
			var (size, type) = i.Type.PrimitiveType switch {
				PrimitiveType.Float32 => (sizeof(float), VertexAttribPointerType.Float),
				var x when true => throw new Exception( $"Unknown data type: {x}" )
			};

			GL.VertexAttribPointer( i.Location, length, type, false, stride, offset );
			GL.EnableVertexAttribArray( i.Location );
			offset += length * size;
		}
	}

	DrawElementsType indexType;
	public void BindIndexBuffer ( IBuffer buffer ) {
		if ( vao == 0 ) {
			vao = GL.GenVertexArray();
			GL.BindVertexArray( vao );
		}

		GL.BindBuffer( BufferTarget.ElementArrayBuffer, ((IGlObject)buffer).Handle );
		indexType = buffer is Buffer<uint> ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		GL.DrawElements( BeginMode.Triangles, (int)vertexCount, indexType, (int)offset );
	}

	public void Dispose () {
		if ( vao != 0 ) {
			GL.BindVertexArray( 0 );
			GL.DeleteVertexArray( vao );
			vao = 0;
		}
	}
}
