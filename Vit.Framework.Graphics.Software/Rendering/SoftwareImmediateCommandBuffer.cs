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
using Vit.Framework.Graphics.Software.Uniforms;

namespace Vit.Framework.Graphics.Software.Rendering;

public class SoftwareImmadiateCommandBuffer : IImmediateCommandBuffer {
	TargetImage renderTarget = null!;
	public DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, ColorRgba<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null ) {
		renderTarget = (TargetImage)framebuffer;
		var color = ( clearColor ?? ColorRgba.Black ).ToByte().BitCast<ColorRgba<byte>, Rgba32>();
		renderTarget.AsSpan().Fill( color );
		if ( clearDepth != null || clearStencil != null ) {
			renderTarget.DepthStencilAsSpan().Fill( new() {
				Depth = clearDepth ?? 0,
				Stencil = (byte)( clearStencil ?? 0 )
			} );
		}

		return new DisposeAction<ICommandBuffer>( this, static self => {
			( (SoftwareImmadiateCommandBuffer)self ).renderTarget = null!;
		} );
	}

	public void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) where T : unmanaged {
		( (Buffer<T>)buffer ).Upload( data, offset );
	}

	public void UploadTextureData<TPixel> ( ITexture texture, ReadOnlySpan<TPixel> data ) where TPixel : unmanaged {
		MemoryMarshal.Cast<TPixel,byte>( data ).CopyTo( MemoryMarshal.Cast<Rgba32, byte>( ((Texture)texture).AsSpan() ) );
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

	BufferTest depthTest;
	public void SetDepthTest ( BufferTest test ) {
		depthTest = test;
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
		var uniforms = ((UniformSet)shaders!.GetUniformSet(0)).UniformBuffers;
		foreach ( var (binding, uniform) in shaders!.Shaders.SelectMany( x => x.UniformsByBinding ).DistinctBy( x => x.Key ) ) {
			var address = shaders!.Uniforms[binding];

			var data = uniforms[binding];
			data.buffer.Bytes.Slice( (int)data.offset, (int)data.stride ).CopyTo( memory.GetMemory( address, (int)data.stride ) );
		}

		foreach ( var (binding, uniform) in shaders.Shaders.SelectMany( x => x.UniformConstantsByBinding ).DistinctBy( x => x.Key ) ) {
			var address = shaders!.Uniforms[binding];

			memory.Write( address, value: binding );
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

	bool testDepth ( ref D24S8 pixel, float depth ) {
		if ( !depthTest.IsEnabled )
			return true;

		bool testValue = depthTest.CompareOperation switch {
			CompareOperation.LessThan => depth < pixel.Depth,
			CompareOperation.GreaterThan => depth > pixel.Depth,
			CompareOperation.Equal => depth == pixel.Depth,
			CompareOperation.NotEqual => depth != pixel.Depth,
			CompareOperation.LessThanOrEqual => depth <= pixel.Depth,
			CompareOperation.GreaterThanOrEqual => depth >= pixel.Depth,
			CompareOperation.Always => true,
			CompareOperation.Never or _ => false
		};

		if ( depthTest.WriteOnPass && testValue )
			pixel.Depth = depth;

		return testValue;
	}

	void rasterize ( VertexShaderOutput A, VertexShaderOutput B, VertexShaderOutput C, SoftwareFragmentShader frag, ShaderMemory memory ) {
		var resultSize = renderTarget.Size.Cast<float>();
		static Point3<float> project ( Vector4<float> vector, Size2<float> size ) {
			var result = vector.XYZ / vector.W;
			return new Point3<float>() {
				X = ( result.X + 1 ) * 0.5f * size.Width,
				Y = ( result.Y + 1 ) * 0.5f * size.Height,
				Z = result.Z
			};
		}

		var a = project( A.Position, resultSize );
		var b = project( B.Position, resultSize );
		var c = project( C.Position, resultSize );

		var min = new Point2<int> {
			X = int.Max( int.Min( int.Min( (int)a.X, (int)b.X ), (int)c.X ), 0 ),
			Y = int.Max( int.Min( int.Min( (int)a.Y, (int)b.Y ), (int)c.Y ), 0 )
		};
		var max = new Point2<int> {
			X = int.Min( int.Max( int.Max( (int)a.X, (int)b.X ), (int)c.X ), (int)renderTarget.Size.Width - 1 ),
			Y = int.Min( int.Max( int.Max( (int)a.Y, (int)b.Y ), (int)c.Y ), (int)renderTarget.Size.Height - 1 )
		};

		var batch = new Triangle.BarycentricBatch<float>( a.XY, b.XY, c.XY );
		var pixelSpan = renderTarget.AsSpan2D();
		var depthSpan = renderTarget.DepthStencilAsSpan2D();
		for ( int y = min.Y; y <= max.Y; y++ ) {
			var pixelRow = pixelSpan.GetRow( y );
			var depthRow = depthSpan.GetRow( y );
			for ( int x = min.X; x <= max.X; x++ ) {
				var point = new Point2<float>( x, y );

				var (bA, bB, bC) = batch.Calculate( point );
				if ( bA < 0 || bB < 0 || bC < 0 )
					continue;

				var depth = a.Z * bA + b.Z * bB + c.Z * bC;
				if ( !testDepth( ref depthRow[x], depth ) )
					continue;

				shaders!.VertexStageLinkage.Interpolate( bA, bB, bC, memory );
				var fragData = frag.Execute( memory );
				pixelRow[x] = fragData.Color.ToByte().BitCast<ColorRgba<byte>, Rgba32>();
			}
		}
	}

	public void Dispose () {

	}
}
