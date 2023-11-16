using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class SvgFragment {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Svg Fragment" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec4 inColor;

		layout(location = 0) out vec4 outColor;

		void main () {
			outColor = inColor;
		}
	", ShaderLanguage.GLSL, ShaderPartType.Fragment );
}
