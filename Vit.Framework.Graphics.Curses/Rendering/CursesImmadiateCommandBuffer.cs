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

	ShaderStageOutput[] vertexAttributes = new ShaderStageOutput[] {
		new() { Outputs = new() },
		new() { Outputs = new() },
		new() { Outputs = new() }
	};
	ShaderStageOutput interpolated = new() { Outputs = new() };
	void initializeAttributes ( SoftwareShader shader ) {
		foreach ( var i in vertexAttributes.Append( interpolated ) ) {
			i.Outputs.Clear();
			foreach ( var (location, output) in shader.OutputsByLocation ) {
				i.Outputs.Add( location, output.Type.Base.CreateVariable() );
			}
		}
	}

	IEnumerable<uint> enumerateIndices ( uint count, uint offset ) {
		var indexBuffer = (IByteBuffer)this.indexBuffer!;
		bool isShort = indexBuffer is Buffer<ushort>;

		for ( uint i = 0; i < count; i++ ) {
			uint index;
			if ( isShort )
				index = MemoryMarshal.Cast<byte, ushort>( indexBuffer.Bytes )[(int)( offset + i )];
			else
				index = MemoryMarshal.Cast<byte, uint>( indexBuffer.Bytes )[(int)( offset + i )];
			yield return index;
		}
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		var vertexBuffer = (IByteBuffer)this.vertexBuffer!;
		var shaders = this.shaders!.Shaders.Select( x => x.SoftwareShader );

		var vert = (SoftwareVertexShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Vertex ).SoftwareShader;
		var frag = (SoftwareFragmentShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Fragment ).SoftwareShader;
		setUniforms();

		initializeAttributes( vert );

		var indices = enumerateIndices( vertexCount, offset ).GetEnumerator();
		var vertexBytes = vertexBuffer.Bytes;
		if ( topology == Topology.Triangles ) {
			while ( indices.MoveNext() ) {
				var indexA = indices.Current;
				indices.MoveNext();
				var indexB = indices.Current;
				indices.MoveNext();
				var indexC = indices.Current;

				var a = vert.Execute( vertexBytes, indexA, ref vertexAttributes[0] );
				var b = vert.Execute( vertexBytes, indexB, ref vertexAttributes[1] );
				var c = vert.Execute( vertexBytes, indexC, ref vertexAttributes[2] );

				rasterize( a, b, c, frag );
			}
		}
		else {
			throw new InvalidOperationException( $"Unsupported topology: {topology}" );
		}
	}

	void setUniforms () {
		var uniforms = shaders!.UniformBuffers;
		foreach ( var i in shaders!.Shaders ) {
			foreach ( var (binding, data) in uniforms ) {
				i.SoftwareShader.SetUniforms( binding, data.buffer.Bytes.Slice( (int)data.offset, (int)data.stride ) );
			}
		}
	}

	void rasterize ( VertexShaderOutput A, VertexShaderOutput B, VertexShaderOutput C, SoftwareFragmentShader frag ) {
		var resultSize = renderTarget.Size.Cast<float>();
		static Point3<float> project ( Vector4<float> vector, Size2<float> size ) {
			var point = ( vector.XYZ / vector.W ).FromOrigin();
			return point with {
				X = ( point.X + 1 ) * 0.5f * size.Width,
				Y = ( point.Y + 1 ) * 0.5f * size.Height
			};
		}

		var a = project( A.Position, resultSize );
		var b = project( B.Position, resultSize );
		var c = project( C.Position, resultSize );

		// sort them by Y in ascending order
		if ( c.Y < b.Y )
			(c, b, vertexAttributes[1], vertexAttributes[2]) = (b, c, vertexAttributes[2], vertexAttributes[1]);
		if ( b.Y < a.Y )
			(b, a, vertexAttributes[1], vertexAttributes[0]) = (a, b, vertexAttributes[0], vertexAttributes[1]);
		if ( c.Y < b.Y )
			(c, b, vertexAttributes[1], vertexAttributes[2]) = (b, c, vertexAttributes[2], vertexAttributes[1]);

		if ( a.Y == c.Y )
			return; // degenerate triangle

		//     * C
		//    / \
		//    |  \
		//    |   \
		//   /     \
		// B *______* D
		//      \__  \
		//         \__* A

		var progress = ( b.Y - a.Y ) / ( c.Y - a.Y );
		var d = new Point3<float>() {
			X = a.X * ( 1 - progress ) + c.X * progress,
			Y = b.Y,
			Z = a.Z * ( 1 - progress ) + c.Z * progress
		};

		// top triangle
		var startX = Math.Min( b.X, d.X );
		var endX = Math.Max( b.X, d.X );
		var startXStep = ( c.X - startX ) / ( c.Y - b.Y );
		var endXStep = ( c.X - endX ) / ( c.Y - b.Y );
		for ( int y = (int)b.Y; y <= c.Y; y++ ) {
			for ( int x = (int)Math.Ceiling(startX); x < endX; x++ ) {
				if ( x < 0 || x >= resultSize.Width || y < 0 || y >= resultSize.Height )
					continue;

				var point = new Point2<float>( x, y );
				var (bA, bB, bC) = Triangle.GetBarycentric( a.XY, b.XY, c.XY, point );
				interpolated.Interpolate( bA, bB, bC, vertexAttributes[0], vertexAttributes[1], vertexAttributes[2] );

				var fragData = frag.Execute( interpolated );
				renderTarget.Pixels[y, x].Background = fragData.Color.ToByte();
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
			for ( int x = (int)Math.Ceiling(startX); x < endX; x++ ) {
				if ( x < 0 || x >= resultSize.Width || y < 0 || y >= resultSize.Height )
					continue;

				var point = new Point2<float>( x, y );
				var (bA, bB, bC) = Triangle.GetBarycentric( a.XY, b.XY, c.XY, point );
				interpolated.Interpolate( bA, bB, bC, vertexAttributes[0], vertexAttributes[1], vertexAttributes[2] );

				var fragData = frag.Execute( interpolated );
				renderTarget.Pixels[y, x].Background = fragData.Color.ToByte();
			}

			startX += startXStep;
			endX += endXStep;
		}
	}

	public void Dispose () {
		
	}
}
