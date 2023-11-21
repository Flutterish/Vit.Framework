using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Rendering.Shaders.Types;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class TextVertex {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Text Vertex" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec4 inRect;
		layout(location = 1) in vec4 inUvRect;
		layout(location = 2) in float inIgnoreTint;
		layout(location = 3) in vec2 inCorner;

		layout(location = 0) out vec2 outUv;
		layout(location = 1) out vec2 outUvRange;
		layout(location = 2) out float outIgnoreTint;
		layout(location = 3) out vec2 outModelSpace;

		layout(binding = 0, set = 0) uniform GlobalUniforms {
			mat3 proj;
			uvec2 screenSize;
		} globalUniforms;

		layout(binding = 0, set = 1) uniform Uniforms {
			mat3 model;
			vec4 tint;
			uint maskingPtr;
		} uniforms;

		vec2 toScreenSpace ( vec2 ndc ) {
			return (ndc + 1) / 2 * globalUniforms.screenSize;
		}
	
		vec2 toNdc ( vec2 screenSpace ) {
			return screenSpace / globalUniforms.screenSize * 2 - 1;
		}

		void main () {
			outIgnoreTint = inIgnoreTint;

			vec2 screenSpaceBase = toScreenSpace( (globalUniforms.proj * uniforms.model * vec3(inRect.xy, 1)).xy );
			vec2 screenSpaceEnding = toScreenSpace( (globalUniforms.proj * uniforms.model * vec3(inRect.xy + inRect.zw, 1)).xy );
			vec2 size = floor(screenSpaceEnding - screenSpaceBase);

			vec2 positionAligned = round(screenSpaceBase) + size * inCorner;

			outUv = inUvRect.xy + inUvRect.zw * inCorner;
			outUvRange = inUvRect.zw / size;
			outModelSpace = (uniforms.model * vec3(inRect.xy + inRect.zw * inCorner, 1)).xy;
			gl_Position = vec4(toNdc(positionAligned), 0, 1);
		}
	", ShaderLanguage.GLSL, ShaderPartType.Vertex ); // TODO the outModelSpace is not exactly accurate but its okay for now

	static VertexInputDescription? inputDescription;
	public static VertexInputDescription InputDescription => inputDescription ??= VertexInputDescription.CreateGrouped( Spirv.Reflections, 
		(BufferInputRate.PerInstance, new uint[] { 0, 1, 2 }),
		(BufferInputRate.PerVertex, new uint[] { 3 })
	);

	/// <summary>
	/// Per-instance data for a quad (binding 0)
	/// </summary>
	public struct Vertex {
		public required UniformRectangle<float> Rectangle;
		public required UniformRectangle<float> UvRectangle;
		public required UniformFloatBool IgnoreTint;
	}

	/// <summary>
	/// Corner values (binding 1)
	/// </summary>
	public struct Corner {
		public required Axes2<float> Value;
	}

	public struct Uniforms {
		public required Matrix4x3<float> Matrix;
		public required ColorSRgba<float> Tint;
		public required uint MaskingPointer;
	}
}
