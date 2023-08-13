using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Software.Buffers;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Software.Rendering;

public class SoftwareImmadiateCommandBuffer : BasicCommandBuffer<SoftwareRenderer, TargetImage, Texture, ShaderSet>, IImmediateCommandBuffer {
	public SoftwareImmadiateCommandBuffer ( SoftwareRenderer renderer ) : base( renderer ) { }

	TargetImage renderTarget = null!;
	protected override DisposeAction<ICommandBuffer> RenderTo ( TargetImage framebuffer, ColorRgba<float> clearColor, float clearDepth, uint clearStencil ) {
		renderTarget = framebuffer;
		var color = clearColor.ToByte().BitCast<ColorRgba<byte>, Rgba32>();
		renderTarget.AsSpan().Fill( color );
		renderTarget.DepthStencilAsSpan().Fill( new() {
			Depth = clearDepth,
			Stencil = (byte)clearStencil
		} );

		return new DisposeAction<ICommandBuffer>( this, static self => {
			( (SoftwareImmadiateCommandBuffer)self ).renderTarget = null!;
		} );
	}

	public override void UploadRaw ( IDeviceBuffer buffer, ReadOnlySpan<byte> data, uint offset = 0 ) {
		data.CopyTo( ((IByteBuffer)buffer).Bytes.Slice( (int)offset ) );
	}

	public override void Upload<T> ( IDeviceBuffer<T> buffer, ReadOnlySpan<T> data, uint offset = 0 ) {
		( (Buffer<T>)buffer ).Upload( data, offset );
	}

	protected override void UploadTextureData<TPixel> ( Texture texture, ReadOnlySpan<TPixel> data ) {
		MemoryMarshal.Cast<TPixel,byte>( data ).CopyTo( MemoryMarshal.Cast<Rgba32, byte>( texture.AsSpan() ) );
	}

	IEnumerable<uint> enumerateIndices ( uint count, uint offset ) {
		var indexBuffer = (IByteBuffer)this.IndexBuffer!;
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
		foreach ( var ((set, binding), uniform) in ShaderSet.Shaders.SelectMany( x => x.UniformsByBinding ).DistinctBy( x => x.Key ) ) {
			var address = ShaderSet.Uniforms[(set, binding)];

			var data = ShaderSet.UniformSets[set].UniformBuffers[binding];
			data.buffer.Bytes.Slice( (int)data.offset, (int)data.stride ).CopyTo( memory.GetMemory( address, (int)data.stride ) );
		}

		foreach ( var ((set, binding), uniform) in ShaderSet.Shaders.SelectMany( x => x.UniformConstantsByBinding ).DistinctBy( x => x.Key ) ) {
			var address = ShaderSet.Uniforms[(set, binding)];

			memory.Write( address, value: new OpaqueHandle { Set = set, Binding = binding } );
		}
	}

	IByteBuffer[] vertexBuffers = new IByteBuffer[16];
	uint[] bufferOffsets = new uint[16];
	protected override bool UpdateVertexBufferMetadata ( IBuffer buffer, uint binding, uint offset ) {
		var bufferSet = ((IByteBuffer)buffer).TrySet( ref vertexBuffers[binding] );
		var offsetSet = offset.TrySet( ref bufferOffsets[binding] );

		return bufferSet || offsetSet;
	}

	protected override void DrawIndexed ( uint vertexCount, uint offset = 0 ) {
		using var rentedMemory = new RentedArray<byte>( 1024 );
		var memory = new ShaderMemory { Memory = rentedMemory.AsSpan() };

		//memory.DebugFrame = ShaderSet.BakedDebug;

		var shaders = ShaderSet.Shaders;

		var vert = (SoftwareVertexShader)ShaderSet.Shaders.First( x => x.Type == ShaderPartType.Vertex );
		var frag = (SoftwareFragmentShader)ShaderSet.Shaders.First( x => x.Type == ShaderPartType.Fragment );
		loadUniforms( ref memory );
		beginStage( ShaderSet.VertexStage, ref memory );

		frag.GlobalScope.Opaques.Samplers.Clear();
		foreach ( var (set, uniforms) in ShaderSet.UniformSets ) {
			foreach ( var (binding, sampler) in uniforms.Samplers ) {
				frag.GlobalScope.Opaques.Samplers.Add( new OpaqueHandle { Set = set, Binding = binding }, sampler );
			}
		}

		var indices = enumerateIndices( vertexCount, offset ).GetEnumerator();
		if ( Topology == Topology.Triangles ) {
			while ( indices.MoveNext() ) {
				var indexA = indices.Current;
				indices.MoveNext();
				var indexB = indices.Current;
				indices.MoveNext();
				var indexC = indices.Current;

				var a = execute( vert, indexA, memory, 0 );
				var b = execute( vert, indexB, memory, 1 );
				var c = execute( vert, indexC, memory, 2 );

				beginStage( ShaderSet.FragmentStage, ref memory );
				rasterize( a, b, c, frag, memory );
			}
		}
		else {
			throw new InvalidOperationException( $"Unsupported topology: {Topology}" );
		}
	}

	VertexShaderOutput execute ( SoftwareVertexShader shader, uint index, ShaderMemory memory, int vertexIndex ) {
		foreach ( var (bufferIndex, attributes) in ShaderSet.InputDescription.BufferBindings ) {
			var buffer = vertexBuffers[(int)bufferIndex];
			var offset = (int)(attributes.Stride * index);
			var data = buffer.Bytes;
			data = data.Slice( offset + (int)bufferOffsets[(int)bufferIndex], shader.Stride );

			foreach ( var (location, attribute) in attributes.AttributesByLocation ) {
				var inputId = shader.InputIdByLocation[location];
				var info = ShaderSet.VertexInputs[location];
				var size = info.Type.Size;

				data[..size].CopyTo( memory.GetMemory( info.Address, size ) );
				data = data[size..];
			}
		}

		foreach ( var (location, variable) in ShaderSet.VertexStageLinkage.Variables[vertexIndex] ) {
			var pointerAddress = ShaderSet.VertexStageLinkage.PointerAddresses[location];

			memory.Write( pointerAddress, value: variable.Address );
		}

		return shader.Execute( memory );
	}

	bool testDepthStencil ( ref D24S8 pixel, float depth ) { // TODO something is wrong with this when rendering text (maybe degenerate triangles?)
		if ( !StencilTest.IsEnabled )
			return depthTest( ref pixel, depth );

		var stencil = pixel.Stencil & StencilState.CompareMask;
		var reference = StencilState.ReferenceValue & StencilState.CompareMask;
		bool testValue = StencilTest.CompareOperation switch {
			CompareOperation.LessThan => reference < stencil,
			CompareOperation.GreaterThan => reference > stencil,
			CompareOperation.Equal => reference == stencil,
			CompareOperation.NotEqual => reference != stencil,
			CompareOperation.LessThanOrEqual => reference <= stencil,
			CompareOperation.GreaterThanOrEqual => reference >= stencil,
			CompareOperation.Always => true,
			CompareOperation.Never or _ => false
		};

		if ( testValue ) {
			if ( depthTest( ref pixel, depth ) ) {
				stencilOperation( ref pixel, StencilState.PassOperation );
				return true;
			}
			else {
				stencilOperation( ref pixel, StencilState.DepthFailOperation );
				return false;
			}
		}
		else {
			stencilOperation( ref pixel, StencilState.StencilFailOperation );
			return false;
		}
	}

	void stencilOperation ( ref D24S8 pixel, StencilOperation operation ) {
		byte result = pixel.Stencil;
		switch ( operation ) {
			case StencilOperation.SetTo0:
				result = 0;
				break;

			case StencilOperation.ReplaceWithReference:
				result = (byte)StencilState.ReferenceValue;
				break;

			case StencilOperation.Invert:
				result = (byte)(~pixel.Stencil);
				break;

			case StencilOperation.Increment:
				if ( result != 255 )
					result++;
				break;

			case StencilOperation.Decrement:
				if ( result != 0 )
					result--;
				break;

			case StencilOperation.IncrementWithWrap:
				result++;
				break;

			case StencilOperation.DecrementWithWrap:
				result--;
				break;

			default:
				return;
		}

		pixel.Stencil = (byte)((result & StencilState.WriteMask) | (pixel.Stencil & ~StencilState.WriteMask));
	}

	bool depthTest ( ref D24S8 pixel, float depth ) {
		if ( !DepthTest.IsEnabled )
			return true;

		bool testValue = DepthTest.CompareOperation switch {
			CompareOperation.LessThan => depth < pixel.Depth,
			CompareOperation.GreaterThan => depth > pixel.Depth,
			CompareOperation.Equal => depth == pixel.Depth,
			CompareOperation.NotEqual => depth != pixel.Depth,
			CompareOperation.LessThanOrEqual => depth <= pixel.Depth,
			CompareOperation.GreaterThanOrEqual => depth >= pixel.Depth,
			CompareOperation.Always => true,
			CompareOperation.Never or _ => false
		};

		if ( DepthState.WriteOnPass && testValue )
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
		var (aW, bW, cW) = (1 / A.Position.W, 1 / B.Position.W, 1 / C.Position.W);
		for ( int y = min.Y; y <= max.Y; y++ ) {
			var pixelRow = pixelSpan.GetRow( y );
			var depthRow = depthSpan.GetRow( y );
			for ( int x = min.X; x <= max.X; x++ ) {
				var point = new Point2<float>( x, y );

				var (bA, bB, bC) = batch.Calculate( point );
				if ( bA < 0 || bB < 0 || bC < 0 )
					continue;

				var w = 1 / ( bA * aW + bB * bW + bC * cW );
				bA *= aW * w;
				bB *= bW * w;
				bC *= cW * w;
				var depth = a.Z * bA + b.Z * bB + c.Z * bC;
				if ( !testDepthStencil( ref depthRow[x], depth ) )
					continue;

				ShaderSet.VertexStageLinkage.Interpolate( bA, bB, bC, memory );
				var fragData = frag.Execute( memory );
				pixelRow[x] = fragData.Color.ToByte().BitCast<ColorRgba<byte>, Rgba32>();
			}
		}
	}

	public void Dispose () {

	}

	protected override void UpdatePieline ( PipelineInvalidations invalidations ) {
		
	}

	protected override void UpdateBuffers ( BufferInvalidations invalidations ) {

	}
}
