using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareVertexShader : SoftwareShader {
	public readonly int Stride;
	int PositionOutputOffset;
	public SoftwareVertexShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		Stride = InputsByLocation.Sum( x => x.Value.Base.Size );
		PositionOutputOffset = BuiltinOutputOffsets[0];
	}

	public VertexShaderOutput Execute ( ShaderMemory memory ) {
		loadConstants( ref memory );
		Entry.Call( memory );

		var builtinPtr = memory.Read<int>( GlobalScope.VariableInfo[OutputsWithoutLocation.Single().id].Address );
		return new() {
			Position = memory.Read<Vector4<float>>( builtinPtr + PositionOutputOffset )
		};
	}
}

public struct VertexShaderOutput {
	public Vector4<float> Position;
}