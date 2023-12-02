using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Memory;
using PrimitiveType = Vit.Framework.Graphics.Rendering.Shaders.Reflections.PrimitiveType;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class VertexInputLayout : DisposableObject {
	public readonly int VAO;
	public readonly int BindingPoints;
	int[] strides;
	public VertexInputLayout ( VertexInputDescription vertexInput ) {
		VAO = GL.GenVertexArray();
		GL.BindVertexArray( VAO );

		BindingPoints = vertexInput.BufferBindings.Any() ? (int)vertexInput.BufferBindings.Max( x => x.Key ) + 1 : 0;
		strides = new int[BindingPoints];

		foreach ( var (buffer, attributes) in vertexInput.BufferBindings ) {
			foreach ( var (location, attribute) in attributes.AttributesByLocation ) {
				var format = (attribute.DataType.PrimitiveType, attribute.DataType.Dimensions) switch {
					(PrimitiveType.UInt32, []) => VertexAttribType.UnsignedInt,
					(PrimitiveType.Float32, []) => VertexAttribType.Float,
					(PrimitiveType.Float32, [2, ..]) => VertexAttribType.Float,
					(PrimitiveType.Float32, [3, ..]) => VertexAttribType.Float,
					(PrimitiveType.Float32, [4, ..]) => VertexAttribType.Float,
					_ => throw new Exception( "Unrecognized format" )
				};

				for ( uint i = 0; i < attribute.Locations; i++ ) {
					GL.EnableVertexAttribArray( location + i );
					GL.VertexAttribFormat( (int)(location + i), (int)attribute.LocationElementSize, format, false, (int)(attribute.Offset + i * attribute.LocationByteSize) );
					GL.VertexAttribBinding( location + i, buffer );
				}
			}

			strides[buffer] = (int)attributes.Stride;
			GL.VertexBindingDivisor( (int)buffer, attributes.InputRate == BufferInputRate.PerVertex ? 0 : 1 );
		}
	}

	public void BindVAO () {
		GL.BindVertexArray( VAO );
	}

	public unsafe void BindBuffers ( ReadOnlySpan<int> buffers, ReadOnlySpan<nint> offsets ) {
		fixed ( int* buffersPtr = buffers ) {
			fixed ( nint* offsetsPtr = offsets ) {
				fixed ( int* stridesPtr = strides ) {
					GL.BindVertexBuffers( 0, BindingPoints, buffersPtr, offsetsPtr, stridesPtr );
				}
			}
		}
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteVertexArray( VAO );
	}
}
