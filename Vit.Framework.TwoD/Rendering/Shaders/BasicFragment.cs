using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class BasicFragment {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Basic Fragment" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inUv;

		layout(location = 0) out vec4 outColor;

		layout(binding = 1, set = 1) uniform sampler2D texSampler;
		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
		} uniforms;

		void main () {
			outColor = texture( texSampler, inUv ) * uniforms.tint;
		}
	", ShaderLanguage.GLSL, ShaderPartType.Fragment );
}
