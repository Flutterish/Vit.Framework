using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class Shader : IShaderPart {
	public ShaderPartType Type => spirv.Type;
	public ShaderInfo ShaderInfo => spirv.Reflections;
	SpirvBytecode spirv;
	public Shader ( SpirvBytecode spirv ) {
		this.spirv = spirv;
	}

	public void Dispose () {
		
	}
}
