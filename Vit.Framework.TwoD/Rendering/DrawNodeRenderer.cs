using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Threading.Synchronisation;

namespace Vit.Framework.TwoD.Rendering;

public class DrawNodeRenderer {
	TripleBuffer drawNodeSwapchain = new();
	public readonly IHasDrawNodes<DrawNode> Root;

	DrawNode?[] drawNodes = new DrawNode?[3];
	public DrawNodeRenderer ( IHasDrawNodes<DrawNode> root ) {
		Root = root;
	}

	public void CollectDrawData ( Action<int>? action = null ) {
		using var _ = drawNodeSwapchain.GetForWrite( out var index );
		if ( Root.IsDisposed )
			drawNodes[index] = null;
		else
			drawNodes[index] = Root.GetDrawNode( index );
		action?.Invoke( index );
	}

	public void Draw ( ICommandBuffer commands, Action<int>? action = null ) {
		using var _ = drawNodeSwapchain.GetForRead( out var index, out var _ );
		action?.Invoke( index );
		draw( index, commands );
	}
	public bool DrawIfNew ( ICommandBuffer commands, Action<int>? action = null ) {
		if ( !drawNodeSwapchain.TryGetForRead( out var index, out var dispose ) )
			return false;

		using var _ = dispose;
		action?.Invoke( index );
		draw( index, commands );
		return true;
	}

	void draw ( int index, ICommandBuffer commands ) {
		drawNodes[index]?.Draw( commands );
	}
}

public static class BasicVertexShader {
	public static readonly ShaderIdentifier Identifier = new () { Name = "Vertex" };
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
}

public static class BasicFragmentShader {
	public static readonly ShaderIdentifier Identifier = new() { Name = "Fragment" };
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