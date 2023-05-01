using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Shaders;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class Shader : IShaderPart {
	public ShaderPartType Type => SoftwareShader.Type;
	public ShaderInfo ShaderInfo => SoftwareShader.ShaderInfo;
	SoftwareShader SoftwareShader;
	public Shader ( SpirvBytecode spirv ) {
		SoftwareShader = new( spirv );
	}

	public void Dispose () {
		
	}
}
