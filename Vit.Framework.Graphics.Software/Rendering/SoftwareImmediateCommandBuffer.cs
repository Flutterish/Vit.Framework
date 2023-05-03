using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Rendering;


public class SoftwareImmadiateCommandBuffer : IImmediateCommandBuffer {
	TargetImage renderTarget = null!;
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		renderTarget = (TargetImage)framebuffer;
		var color = ( clearColor ?? ColorRgba.Black ).ToByte().BitCast<ColorRgba<byte>, Rgba32>();
		renderTarget.AsSpan().Fill( color );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			( (SoftwareImmadiateCommandBuffer)self ).renderTarget = null!;
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		( (Buffer<T>)buffer ).Upload( data, offset );
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

	void beginStage ( ShaderSet.BakedStageInfo stageInfo, ref ShaderMemory memory ) {
		memory.StackPointer = stageInfo.StackPointer;
		foreach ( var i in stageInfo.PointerAdresses ) {
			memory.Write( i.ptrAddress, value: i.address );
		}
	}

	void loadUniforms ( ref ShaderMemory memory ) {
		var uniforms = shaders!.UniformBuffers;
		foreach ( var (binding, uniform) in shaders.Shaders.SelectMany( x => x.UniformsByBinding ).DistinctBy( x => x.Key ) ) {
			var address = shaders!.Uniforms[binding];

			var data = uniforms[binding];
			data.buffer.Bytes.Slice( (int)data.offset, (int)data.stride ).CopyTo( memory.GetMemory( address, (int)data.stride ) );
		}
	}

	public void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		using var rentedMemory = new RentedArray<byte>( 1024 );
		var memory = new ShaderMemory { Memory = rentedMemory.AsSpan() };

		var vertexBuffer = (IByteBuffer)this.vertexBuffer!;
		var shaders = this.shaders!.Shaders;

		var vert = (SoftwareVertexShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Vertex );
		var frag = (SoftwareFragmentShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Fragment );
		loadUniforms( ref memory );
		beginStage( this.shaders!.VertexStage, ref memory );

		var indices = enumerateIndices( vertexCount, offset ).GetEnumerator();
		var vertexBytes = vertexBuffer.Bytes;
		if ( topology == Topology.Triangles ) {
			while ( indices.MoveNext() ) {
				var indexA = indices.Current;
				indices.MoveNext();
				var indexB = indices.Current;
				indices.MoveNext();
				var indexC = indices.Current;

				var a = execute( vert, vertexBytes, indexA, memory, 0 );
				var b = execute( vert, vertexBytes, indexB, memory, 1 );
				var c = execute( vert, vertexBytes, indexC, memory, 2 );

				beginStage( this.shaders!.FragmentStage, ref memory );
				rasterize( a, b, c, frag, memory );
			}
		}
		else {
			throw new InvalidOperationException( $"Unsupported topology: {topology}" );
		}
	}

	VertexShaderOutput execute ( SoftwareVertexShader shader, ReadOnlySpan<byte> data, uint index, ShaderMemory memory, int vertexIndex ) {
		var offset = shader.Stride * (int)index;
		data = data.Slice( offset, shader.Stride );
		foreach ( var (location, id) in shader.InputIdByLocation.OrderBy( x => x.Key ) ) {
			var info = shaders!.VertexInputs[location];
			var size = info.Type.Size;
			data[..size].CopyTo( memory.GetMemory( info.Address, size ) );
			data = data[size..];
		}

		foreach ( var (location, variable) in shaders!.VertexStageLinkage.Variables[vertexIndex] ) {
			var pointerAddress = shaders!.VertexStageLinkage.PointerAddresses[location];

			memory.Write( pointerAddress, value: variable.Address );
		}

		return shader.Execute( memory );
	}

	void rasterize ( VertexShaderOutput A, VertexShaderOutput B, VertexShaderOutput C, SoftwareFragmentShader frag, ShaderMemory memory ) {
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
		var indexA = 0;
		var indexB = 1;
		var indexC = 2;

		// sort them by Y in ascending order
		if ( c.Y < b.Y )
			(c, b, indexB, indexC) = (b, c, indexC, indexB);
		if ( b.Y < a.Y )
			(b, a, indexB, indexA) = (a, b, indexA, indexB);
		if ( c.Y < b.Y )
			(c, b, indexB, indexC) = (b, c, indexC, indexB);

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
		var pixels = renderTarget.AsSpan2D();
		for ( int y = (int)b.Y; y <= c.Y; y++ ) {
			for ( int x = (int)Math.Ceiling( startX ); x < endX; x++ ) {
				if ( x < 0 || x >= resultSize.Width || y < 0 || y >= resultSize.Height )
					continue;

				var point = new Point2<float>( x, y );
				var (bA, bB, bC) = Triangle.GetBarycentric( a.XY, b.XY, c.XY, point );
				Span<float> weights = stackalloc[] { bA, bB, bC };
				shaders!.VertexStageLinkage.Interpolate( weights[indexA], weights[indexB], weights[indexC], memory );

				var fragData = frag.Execute( memory );
				pixels[x, y] = fragData.Color.ToByte().BitCast<ColorRgba<byte>, Rgba32>();
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
			for ( int x = (int)Math.Ceiling( startX ); x < endX; x++ ) {
				if ( x < 0 || x >= resultSize.Width || y < 0 || y >= resultSize.Height )
					continue;

				var point = new Point2<float>( x, y );
				var (bA, bB, bC) = Triangle.GetBarycentric( a.XY, b.XY, c.XY, point );
				Span<float> weights = stackalloc[] { bA, bB, bC };
				shaders!.VertexStageLinkage.Interpolate( weights[indexA], weights[indexB], weights[indexC], memory );

				var fragData = frag.Execute( memory );
				pixels[x, y] = fragData.Color.ToByte().BitCast<ColorRgba<byte>, Rgba32>();
			}

			startX += startXStep;
			endX += endXStep;
		}
	}

	public void Dispose () {

	}
}
