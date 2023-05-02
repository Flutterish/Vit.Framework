using System.Collections.Immutable;
using Vit.Framework.Graphics.Direct3D11.Buffers;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Memory;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Vit.Framework.Graphics.Direct3D11.Shaders;

public class ShaderSet : DisposableObject, IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<Shader> Shaders;

	public readonly ID3D11InputLayout? Layout;
	public readonly ID3D11DeviceContext Context;
	public readonly int Stride;
	public ShaderSet ( IEnumerable<IShaderPart> parts, ID3D11DeviceContext context ) {
		Context = context;
		Shaders = parts.Select( x => (Shader)x ).ToImmutableArray();

		var vert = Shaders.OfType<VertexShader>().FirstOrDefault();
		if ( vert is null )
			return;

		var inputs = new InputElementDescription[vert.ShaderInfo.Input.Resources.Count];
		int i = 0;
		int offset = 0;
		foreach ( var vertex in vert.ShaderInfo.Input.Resources.OrderBy( x => x.Location ) ) {
			var (size, format) = (vertex.Type.PrimitiveType, vertex.Type.Dimensions) switch {
				(PrimitiveType.Float32, [2]) => (sizeof(float), Format.R32G32_Float),
				(PrimitiveType.Float32, [3]) => (sizeof(float), Format.R32G32B32_Float),
				_ => throw new Exception( "Unrecognized format" )
			};
			size *= (int)vertex.Type.FlattendedDimensions;

			inputs[i++] = new() {
				SemanticName = "TEXCOORD",
				SemanticIndex = (int)vertex.Location,
				Format = format,
				Slot = 0,
				AlignedByteOffset = offset,
				Classification = InputClassification.PerVertexData,
				InstanceDataStepRate = 0
			};

			offset += size;
		}

		Stride = offset;
		Layout = vert.Handle.Device.CreateInputLayout( inputs, vert.Source.Span );
	}

	public void SetUniformBuffer<T> ( IBuffer<T> buffer, uint binding = 0, uint offset = 0 ) where T : unmanaged {
		Context.VSSetConstantBuffer( (int)binding, ((Buffer<T>)buffer).Handle );
		// TODO check which shaders need it set
	}

	protected override void Dispose ( bool disposing ) {
		Layout?.Dispose();
	}
}
