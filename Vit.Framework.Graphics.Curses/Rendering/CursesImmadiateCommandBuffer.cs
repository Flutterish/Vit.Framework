using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
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
using Vit.Framework.Mathematics.LinearAlgebra;
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
		var vertexBuffer = (IByteBuffer)this.vertexBuffer!;
		var indexBuffer = (IByteBuffer)this.indexBuffer!;
		bool isShort = indexBuffer is Buffer<ushort>;
		var shaders = this.shaders!.Shaders.Select( x => x.SoftwareShader );

		var vert = (SoftwareVertexShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Vertex ).SoftwareShader;

		using RentedArray<VertexShaderOutput> vertices = new( vertexCount );
		var bytes = vertexBuffer.Bytes;
		var indexBytes = indexBuffer.Bytes;
		for ( uint i = 0; i < vertexCount; i++ ) {
			uint index;
			if ( isShort )
				index = MemoryMarshal.Cast<byte, ushort>( indexBytes )[(int)(offset + i)];
			else
				index = MemoryMarshal.Cast<byte, uint>( indexBytes )[(int)(offset + i)];
			vertices[i] = vert.Execute( bytes, index );
		}

		static Point3<float> project ( Vector4<float> vector, Size2<float> size ) {
			var point = (vector.XYZ / vector.W).FromOrigin();
			return point with {
				X = (point.X + 1) * 0.5f * size.Width,
				Y = (point.Y + 1) * 0.5f * size.Height
			};
		}

		var resultSize = renderTarget.Size.Cast<float>();
		if ( topology == Topology.Triangles ) {
			for ( uint i = 0; i < vertexCount; i += 3 ) {
				var a = project( vertices[i].Position, resultSize );
				var b = project( vertices[i + 1].Position, resultSize );
				var c = project( vertices[i + 2].Position, resultSize );

				// sort them by Y in ascending order
				if ( c.Y < b.Y )
					(c, b) = (b, c);
				if ( b.Y < a.Y )
					(b, a) = (a, b);
				if ( c.Y < b.Y )
					(c, b) = (b, c);

				if ( a.Y == c.Y )
					continue; // degenerate triangle

				//     * C
				//    / \
				//    |  \
				//    |   \
				//   /     \
				// B *______* D
				//      \__  \
				//         \__* A

				var progress = (b.Y - a.Y) / (c.Y - a.Y);
				var d = new Point3<float>() {
					X = a.X * (1 - progress) + c.X * progress,
					Y = b.Y,
					Z = a.Z * (1 - progress) + c.Z * progress
				};

				// top triangle
				var startX = Math.Min( b.X, d.X );
				var endX = Math.Max( b.X, d.X );
				var startXStep = ( c.X - startX ) / ( c.Y - b.Y );
				var endXStep = ( d.X - endX ) / ( c.Y - b.Y );
				for ( int y = (int)b.Y; y <= c.Y; y++ ) {
					for ( int x = (int)startX; x < endX; x++ ) {
						renderTarget.Pixels[y, x].Background = ColorRgba.HotPink.ToByte();
					}

					startX += startXStep;
					endX += endXStep;
				}

				// bottom triangle
				startX = Math.Min( b.X, d.X );
				endX = Math.Max( b.X, d.X );
				startXStep = ( a.X - startX ) / ( b.Y - a.Y );
				endXStep = ( a.X - endX ) / ( b.Y - a.Y );
				startX += startXStep;
				endX += endXStep;
				for ( int y = (int)b.Y - 1; y >= a.Y; y-- ) {
					for ( int x = (int)startX; x < endX; x++ ) {
						renderTarget.Pixels[y, x].Background = ColorRgba.HotPink.ToByte();
					}

					startX += startXStep;
					endX += endXStep;
				}
			}
		}
		else {
			throw new InvalidOperationException( $"Unsupported topology: {topology}" );
		}
	}

	public void Dispose () {
		
	}
}
