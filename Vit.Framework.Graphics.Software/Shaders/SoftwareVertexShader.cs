using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareVertexShader : SoftwareShader {
	public SoftwareVertexShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		
	}

	public VertexShaderOutput Execute ( ReadOnlySpan<byte> data, uint index ) {
		return new();
	}
}

public struct VertexShaderOutput {
	public Point4<float> Position;
}