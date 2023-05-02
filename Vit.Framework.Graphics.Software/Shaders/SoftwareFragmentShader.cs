using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareFragmentShader : SoftwareShader {
	IVariable<Vector4<float>> ColorOutput;
	public SoftwareFragmentShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		ColorOutput = (IVariable<Vector4<float>>)OutputsByLocation.Values.First( x => x.Type.Base is RuntimeVector4Type<float> ).Address!;
	}

	public FragmentShaderOutput Execute ( ShaderStageOutput vertexStageOutput ) {
		foreach ( var (loc, variable) in vertexStageOutput.Outputs ) {
			InputsByLocation[loc].Address!.Value = variable.Value;
		}

		Entry.Call();

		var color = ColorOutput.Value;
		return new() {
			Color = new() { R = color.X, G = color.Y, B = color.Z, A = color.W }
		};
	}
}

public struct FragmentShaderOutput {
	public ColorRgba<float> Color;
}