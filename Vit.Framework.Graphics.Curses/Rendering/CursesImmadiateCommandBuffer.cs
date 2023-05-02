using System.Text;
using Vit.Framework.Graphics.Curses.Buffers;
using Vit.Framework.Graphics.Curses.Shaders;
using Vit.Framework.Graphics.Curses.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Curses.Rendering;

public class CursesImmadiateCommandBuffer : IImmediateCommandBuffer {
	Sprite renderTarget = null!;
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		renderTarget = (Sprite)framebuffer;
		var color = (clearColor ?? ColorRgba.Black).ToByte();
		renderTarget.AsSpan().Flat.Fill( new CursesPixel {
			Symbol = new Rune( ' ' ),
			Foreground = ColorRgba.White.ToByte(),
			Background = color
		} );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			((CursesImmadiateCommandBuffer)self).renderTarget = null!;
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		((Buffer<T>)buffer).Upload( data, offset );
	}

	ShaderSet? shaders;
	public void SetShaders ( IShaderSet? shaders ) {
		this.shaders = (ShaderSet?)shaders;
	}

	Topology topology;
	public void SetTopology ( Topology topology ) {
		this.topology = topology;
	}

	public void SetViewport ( AxisAlignedBox2<uint> viewport ) {
		// TODO
	}

	public void SetScissors ( AxisAlignedBox2<uint> scissors ) {
		// TODO
	}

	IBuffer? vertexBuffer;
	public void BindVertexBuffer ( IBuffer buffer ) {
		vertexBuffer = buffer;
	}

	IBuffer? indexBuffer;
	public void BindIndexBuffer ( IBuffer buffer ) {
		indexBuffer = buffer;
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		var vertex = (IByteBuffer)vertexBuffer!;
		var index = indexBuffer;
		var shaders = this.shaders!.Shaders.Select( x => x.SoftwareShader );

		var vert = (SoftwareVertexShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Vertex ).SoftwareShader;

		if ( topology == Topology.Triangles ) {
			using RentedArray<VertexShaderOutput> vertices = new( vertexCount );
			for ( int i = 0; i < vertexCount; i++ ) {
				vertices[i] = vert.Execute( vertex.Bytes, (uint)i );
			}
		}
		else {
			throw new InvalidOperationException( $"Unsupported topology: {topology}" );
		}
	}

	public void Dispose () {
		
	}
}
