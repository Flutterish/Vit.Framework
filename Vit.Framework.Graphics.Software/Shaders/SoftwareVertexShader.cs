using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareVertexShader : SoftwareShader {
	readonly int Stride;
	IVariable<Vector4<float>> PositionOutput;
	(uint location, PointerVariable)[] inputs;
	public SoftwareVertexShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		Stride = InputsByLocation.Sum( x => x.Value.Type.Base.Size );
		PositionOutput = (IVariable<Vector4<float>>)BuiltinOutputs[0];
		inputs = InputsByLocation.OrderBy( x => x.Key ).Select( x => (x.Key, x.Value) ).ToArray();
	}

	public VertexShaderOutput Execute ( ReadOnlySpan<byte> data, uint index, ref ShaderStageOutput stageOutput ) {
		var offset = Stride * (int)index;
		data = data.Slice( offset, Stride );
		foreach ( var (location, input) in inputs ) {
			var size = input.Type.Base.Size;
			input.Parse( data[..size] );
			data = data[size..];
		}

		Entry.Call();

		foreach ( var (loc, variable) in stageOutput.Outputs ) {
			variable.Value = OutputsByLocation[loc].Address!.Value;
		}
		return new() {
			Position = PositionOutput.Value
		};
	}
}

public struct VertexShaderOutput {
	public Vector4<float> Position;
}