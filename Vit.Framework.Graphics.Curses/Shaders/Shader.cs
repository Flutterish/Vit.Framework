using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class Shader : IShaderPart {
	public ShaderPartType Type => Spirv.Type;
	public ShaderInfo ShaderInfo => Spirv.Reflections;
	public readonly SpirvBytecode Spirv;
	public readonly SoftwareShader SoftwareShader;
	public Shader ( SpirvBytecode spirv ) {
		Spirv = spirv;
		SoftwareShader = new SpirvCompiler( spirv.Data ).Specialise( Type switch {
			ShaderPartType.Vertex => ExecutionModel.Vertex,
			ShaderPartType.Fragment => ExecutionModel.Fragment,
			_ => throw new ArgumentException( $"Unsupported shader type: {Type}", nameof( spirv ) )
		} );
	}

	public void Dispose () {
		
	}
}
