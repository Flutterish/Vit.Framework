using System.Diagnostics;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareVertexShader : SoftwareShader {
	readonly int Stride;
	IVariable<Vector4<float>> PositionOutput;
	public SoftwareVertexShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		Stride = InputsByLocation.Sum( x => x.Value.Type.Size );
		PositionOutput = (IVariable<Vector4<float>>)BuiltinOutputs[0];
	}

	public VertexShaderOutput Execute ( ReadOnlySpan<byte> data, uint index ) {
		var offset = Stride * (int)index;
		data = data.Slice( offset, Stride );
		foreach ( var (location, input) in InputsByLocation ) {
			var size = input.Type.Size;
			input.Parse( data[..size] );
			data = data[size..];
		}

		Entry.Call();

		return new() {
			Position = PositionOutput.Value
		};
	}
}

public struct VertexShaderOutput {
	public Vector4<float> Position;
}