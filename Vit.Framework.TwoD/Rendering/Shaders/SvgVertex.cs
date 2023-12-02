using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.Rendering.Shaders;

public static class SvgVertex {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Svg Vertex" };
	static SpirvBytecode? spirv;
	public static SpirvBytecode Spirv => spirv ??= new SpirvBytecode( @"#version 450
		layout(location = 0) in vec2 inPosition;
		layout(location = 1) in vec4 inColor;

		layout(location = 0) out vec4 outColor;

		void main () {
			outColor = inColor;
			gl_Position = vec4(inPosition, 0, 1);
		}
	", ShaderLanguage.GLSL, ShaderPartType.Vertex );

	static VertexInputDescription? inputDescription;
	public static VertexInputDescription InputDescription => inputDescription ??= VertexInputDescription.CreateSingle( Spirv.Reflections );
	public static VertexShaderDescription Description => new() { Input = InputDescription, Shader = Identifier };

	public struct Vertex {
		public Point2<float> Position;
		public ColorSRgba<float> Color;
	}
}
