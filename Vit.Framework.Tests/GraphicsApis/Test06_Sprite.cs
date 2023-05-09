using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test06_Sprite : GenericRenderThread {
	DrawableRenderer drawableRenderer;
	ShaderStore shaderStore = new();
	Texture texture;

	Sprite sprite;
	public Test06_Sprite ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
		shaderStore.AddShaderPart( DrawableRenderer.TestVertex, new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPositionAndUv;

			layout(location = 0) out vec2 outUv;

			layout(binding = 0) uniform Uniforms {
				mat3 model;
			} uniforms;

			void main () {
				outUv = inPositionAndUv;
				gl_Position = vec4((uniforms.model * vec3(inPositionAndUv, 1)).xy, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		shaderStore.AddShaderPart( DrawableRenderer.TestFragment, new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inUv;

			layout(location = 0) out vec4 outColor;

			layout(binding = 1) uniform sampler2D texSampler;

			void main () {
				outColor = texture( texSampler, inUv );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

		var image = Image.Load<Rgba32>( "./texture.jpg" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = new( image );

		sprite = new( shaderStore, texture );
		drawableRenderer = new( sprite );
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		shaderStore.CompileNew( commands.Renderer );

		using var _ = commands.RenderTo( framebuffer );
		commands.SetTopology( Topology.Triangles );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		drawableRenderer.CollectDrawData();
		drawableRenderer.Draw( commands );
	}

	protected override void Dispose () {
		sprite.Dispose();

		texture.Dispose();
		shaderStore.Dispose();
	}
}
