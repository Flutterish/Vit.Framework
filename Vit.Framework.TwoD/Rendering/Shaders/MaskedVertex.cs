using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class MaskedVertex {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Masked Vertex" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inPosition;
		layout(location = 1) in vec2 inUv;

		layout(location = 0) out vec2 outUv;
		layout(location = 1) out vec2 outModelSpace;

		layout(binding = 0, set = 0) uniform GlobalUniforms {
			mat3 proj;
			uvec2 screenSize;
		} globalUniforms;

		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
			uint maskingPtr;
		} uniforms;

		void main () {
			outUv = inUv;
			vec3 modelSpace = uniforms.model * vec3(inPosition, 1);
			outModelSpace = modelSpace.xy;
			gl_Position = vec4((globalUniforms.proj * modelSpace).xy, 0, 1);
		}
	", ShaderLanguage.GLSL, ShaderPartType.Vertex );

	static VertexInputDescription? inputDescription;
	public static VertexInputDescription InputDescription => inputDescription ??= VertexInputDescription.CreateSingle( Spirv.Reflections );

	public struct Vertex {
		public Point2<float> Position;
		public Point2<float> UV;

		public Point2<float> PositionAndUV {
			set {
				Position = value;
				UV = value;
			}
		}
	}

	public struct Uniforms {
		public Matrix4x3<float> Matrix;
		public ColorSRgba<float> Tint;
		public uint MaskingPointer;
	}
}
