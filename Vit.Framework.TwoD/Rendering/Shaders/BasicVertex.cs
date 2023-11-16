using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class BasicVertex {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Vertex" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inPositionAndUv;

		layout(location = 0) out vec2 outUv;

		layout(binding = 0, set = 0) uniform GlobalUniforms {
			mat3 proj;
		} globalUniforms;

		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
		} uniforms;

		void main () {
			outUv = inPositionAndUv;
			gl_Position = vec4((globalUniforms.proj * uniforms.model * vec3(inPositionAndUv, 1)).xy, 0, 1);
		}
	", ShaderLanguage.GLSL, ShaderPartType.Vertex );

	static VertexInputDescription? inputDescription;
	public static VertexInputDescription InputDescription => inputDescription ??= VertexInputDescription.CreateSingle( Spirv.Reflections );

	public struct Vertex {
		public Point2<float> PositionAndUV;
	}

	public struct Uniforms {
		public Matrix4x3<float> Matrix;
		public ColorSRgba<float> Tint;
	}
}
