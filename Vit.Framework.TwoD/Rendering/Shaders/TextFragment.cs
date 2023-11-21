using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Shaders;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class TextFragment {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Text Fragment" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inUv;
		layout(location = 1) in vec2 inUvRange;
		layout(location = 2) in float inIgnoreTint;

		layout(location = 0) out vec4 outColor;

		layout(binding = 1, set = 1) uniform sampler2D texSampler;
		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
		} uniforms;

		vec4 sampleAt ( vec2 uv ) {
			const int sampleCount = 4;
			const int totalSamples = sampleCount * sampleCount;
			vec4 result = texture( texSampler, uv );
			for ( int x = 0; x < sampleCount; x++ ) {
				float xOffset = inUvRange.x / (sampleCount + 1) * (x + 0.5) - inUvRange.x / 2;
				for ( int y = 0; y < sampleCount; y++ ) {
					float yOffset = inUvRange.y / (sampleCount + 1) * (y + 0.5) - inUvRange.y / 2;
					result += texture( texSampler, uv + vec2(xOffset, yOffset) );
				}
			}

			return result / (totalSamples + 1);
		}

		void main () {
			float subpixelWidth = inUvRange.x / 3;
			
			vec2 ra = sampleAt( inUv - vec2( subpixelWidth, 0 ) ).ra;
			vec2 ga = sampleAt( inUv ).ga;
			vec2 ba = sampleAt( inUv + vec2( subpixelWidth, 0 ) ).ba;

			outColor = vec4( ra.x, ga.x, ba.x, 1 );
			outColor *= max( max( ra.y, ga.y ), ba.y );
			if ( inIgnoreTint < 0.5 ) {
				outColor *= uniforms.tint;
			}
		}
	", ShaderLanguage.GLSL, ShaderPartType.Fragment ); // TODO subpixel renders assuming RGB LCD
}
