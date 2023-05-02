using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Software.Spirv;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareShader {
	public readonly SpirvBytecode Spirv;
	public ShaderInfo ShaderInfo => Spirv.Reflections;
	public ShaderPartType Type => Spirv.Type;

	public readonly SpirvCompiler Compiler;
	public SoftwareShader ( SpirvBytecode spirv ) {
		Spirv = spirv;
		Compiler = new SpirvCompiler( spirv.Data );
	}
}
