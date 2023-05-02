using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Shaders;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class Shader : IShaderPart {
	public ShaderPartType Type => Spirv.Type;
	public ShaderInfo ShaderInfo => Spirv.Reflections;
	public readonly SpirvBytecode Spirv;
	public readonly SoftwareShader SoftwareShader;
	public Shader ( SpirvBytecode spirv ) {
		Spirv = spirv;
		SoftwareShader = new( spirv );
	}

	public void Dispose () {
		
	}
}
