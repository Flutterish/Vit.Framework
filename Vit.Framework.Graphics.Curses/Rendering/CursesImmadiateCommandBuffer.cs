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
		new() { OutputsByLocation = new() },
		new() { OutputsByLocation = new() },
		new() { OutputsByLocation = new() }
	};
	ShaderStageOutput interpolated = new() { OutputsByLocation = new() };
	Dictionary<uint, VariableInfo> vertexInputsByLocation = new();
	Dictionary<uint, VariableInfo> vertexOutputsByLocation = new();
	void initializeAttributes ( SoftwareVertexShader shader, ref ShaderMemory memory ) {
		int index = 0;
		foreach ( var i in vertexAttributes.Append( interpolated ) ) {
			i.OutputsByLocation.Clear();
			foreach ( var (location, output) in shader.OutputsByLocation ) {
				var variable = memory.StackAlloc( output.Base );
#if SHADER_DEBUG
				memory.AddDebug( new() {
					Variable = variable,
					Name = $"Out (location = {location}) [{(index == 3 ? "Interpolated" : $"Vertex {index}")}]"
				} );
#endif
				i.OutputsByLocation.Add( location, variable );
			}

			index++;
		}

		vertexOutputsByLocation.Clear();
		foreach ( var (location, output) in shader.OutputsByLocation ) {
			var ptr = memory.StackAlloc( output );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"Out (location = {location}) pointer"
			} );
#endif

			shader.GlobalScope.VariableInfo[shader.OutputIdByLocation[location]] = ptr;
			vertexOutputsByLocation.Add( location, ptr );
		}

		foreach ( var (output, id) in shader.OutputsWithoutLocation ) {
			var variable = memory.StackAlloc( output.Base );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"Out (builtin)"
			} );
#endif

			var ptr = memory.StackAlloc( output );
			memory.Write( ptr.Address, value: variable.Address );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"Out (builtin) pointer"
			} );
#endif

			shader.GlobalScope.VariableInfo[id] = ptr;
		}

		vertexInputsByLocation.Clear();
		foreach ( var (location, input) in shader.InputsByLocation ) {
			var variable = memory.StackAlloc( input.Base );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"In (location = {location})"
			} );
#endif
			vertexInputsByLocation.Add( location, variable );

			var ptr = memory.StackAlloc( input );
			memory.Write( ptr.Address, value: variable.Address );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"In (location = {location}) pointer"
			} );
#endif

			shader.GlobalScope.VariableInfo[shader.InputIdByLocation[location]] = ptr;
		}
	}

	void initializeAttributes ( SoftwareFragmentShader shader, ref ShaderMemory memory ) {
		foreach ( var (location, input) in shader.InputsByLocation ) {
			var output = interpolated.OutputsByLocation[location];
			var ptr = memory.StackAlloc( input );
			memory.Write( ptr.Address, value: output.Address );
			shader.GlobalScope.VariableInfo[shader.InputIdByLocation[location]] = ptr;

#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"Frag In (location = {location}) pointer"
			} );
#endif
		}

		foreach ( var (location, output) in shader.OutputsByLocation ) {
			var variable = memory.StackAlloc( output.Base );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"Frag Out (location = {location})"
			} );
#endif

			var ptr = memory.StackAlloc( output );
			memory.Write( ptr.Address, value: variable.Address );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"Frag Out (location = {location}) pointer"
			} );
#endif

			shader.GlobalScope.VariableInfo[shader.OutputIdByLocation[location]] = ptr;
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
		using var rentedMemory = new RentedArray<byte>( 1024 );
		var memory = new ShaderMemory { Memory = rentedMemory.AsSpan() };

		var vertexBuffer = (IByteBuffer)this.vertexBuffer!;
		var shaders = this.shaders!.Shaders.Select( x => x.SoftwareShader );

		var vert = (SoftwareVertexShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Vertex ).SoftwareShader;
		var frag = (SoftwareFragmentShader)this.shaders!.Shaders.First( x => x.Type == ShaderPartType.Fragment ).SoftwareShader;
		setUniforms( ref memory );

		initializeAttributes( vert, ref memory );
		initializeAttributes( frag, ref memory );

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
			var info = vertexInputsByLocation[location];
			var size = info.Type.Size;
			data[..size].CopyTo( memory.GetMemory( info.Address, size ) );
			data = data[size..];
		}
		foreach ( var (location, id) in shader.OutputIdByLocation ) {
			var output = vertexAttributes[vertexIndex].OutputsByLocation[location];
			var ptr = vertexOutputsByLocation[location];
			memory.Write( ptr.Address, value: output.Address );
		}

		return shader.Execute( memory );
	}

	void setUniforms ( ref ShaderMemory memory ) {
		var uniforms = shaders!.UniformBuffers;
		foreach ( var (binding, uniform) in shaders.Shaders.SelectMany( x => x.SoftwareShader.UniformsByBinding ).DistinctBy( x => x.Key ) ) {
			var uniformType = uniform.Base;
			var variable = memory.StackAlloc( uniformType );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = variable,
				Name = $"Uniform (set = {binding})"
			} );
#endif

			var data = uniforms[binding];
			data.buffer.Bytes.Slice( (int)data.offset, (int)data.stride ).CopyTo( memory.GetMemory( variable.Address, (int)data.stride ) );

			var ptr = memory.StackAlloc( uniform );
			memory.Write( ptr.Address, value: variable.Address );
#if SHADER_DEBUG
			memory.AddDebug( new() {
				Variable = ptr,
				Name = $"Uniform (set = {binding}) pointer"
			} );
#endif
			foreach ( var i in shaders!.Shaders ) {
				if ( i.SoftwareShader.UniformIdByBinding.TryGetValue( binding, out var id ) ) {
					i.SoftwareShader.GlobalScope.VariableInfo[id] = ptr;
				}
			}
		}
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
				interpolated.Interpolate( bA, bB, bC, vertexAttributes[0], vertexAttributes[1], vertexAttributes[2], memory );

				var fragData = frag.Execute( memory );
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
				interpolated.Interpolate( bA, bB, bC, vertexAttributes[0], vertexAttributes[1], vertexAttributes[2], memory );

				var fragData = frag.Execute( memory );
				renderTarget.Pixels[y, x].Background = fragData.Color.ToByte();
			}

			startX += startXStep;
			endX += endXStep;
		}
	}

	public void Dispose () {
		
	}
}
