using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using PrimitiveType = Vit.Framework.Graphics.Rendering.Shaders.Reflections.PrimitiveType;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class VertexInputLayout : DisposableObject {
	public int VAO;
	int bindingPoints;
	int[] strides;
	nint[] offsets;
	public VertexInputLayout ( VertexInputDescription vertexInput ) {
		VAO = GL.GenVertexArray();
		GL.BindVertexArray( VAO );

		bindingPoints = vertexInput.BufferBindings.Any() ? (int)vertexInput.BufferBindings.Max( x => x.Key ) + 1 : 0;
		strides = new int[bindingPoints];
		offsets = new nint[bindingPoints];

		foreach ( var (buffer, attributes) in vertexInput.BufferBindings ) {
			foreach ( var (location, attribute) in attributes.AttributesByLocation ) {
				var format = (attribute.DataType.PrimitiveType, attribute.DataType.Dimensions) switch {
					(PrimitiveType.Float32, [2]) => VertexAttribType.Float,
					(PrimitiveType.Float32, [3]) => VertexAttribType.Float,
					_ => throw new Exception( "Unrecognized format" )
				};

				GL.EnableVertexAttribArray( location );
				GL.VertexAttribFormat( (int)location, (int)attribute.DataType.FlattendedDimensions, format, false, (int)attribute.Offset );
				GL.VertexAttribBinding( location, buffer );
			}

			offsets[buffer] = 0;
			strides[buffer] = (int)attributes.Stride;
			GL.VertexBindingDivisor( (int)buffer, attributes.InputRate == BufferInputRate.PerVertex ? 0 : 1 );
		}
	}

	public void BindVAO () {
		GL.BindVertexArray( VAO );
	}

	public unsafe void BindBuffers ( ReadOnlySpan<int> buffers ) {
		GL.BindVertexBuffers( 0, int.Min( buffers.Length, bindingPoints ), buffers.Data(), offsets.Data(), strides.Data() );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteVertexArray( VAO );
	}
}
