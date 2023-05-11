﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel.DataAnnotations;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class TestFinal_2D : GenericRenderThread {
	DrawableRenderer drawableRenderer;
	ShaderStore shaderStore = new();
	Texture texture;

	Container<Drawable> container;
	public TestFinal_2D ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
		shaderStore.AddShaderPart( DrawableRenderer.TestVertex, new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inPositionAndUv;

			layout(location = 0) out vec2 outUv;

			layout(binding = 0, set = 0) uniform GlobalUniforms {
				mat3 proj;
			} globalUniforms;

			layout(binding = 0, set = 1) uniform Uniforms {
				mat3 model;
			} uniforms;

			void main () {
				outUv = inPositionAndUv;
				gl_Position = vec4((globalUniforms.proj * uniforms.model * vec3(inPositionAndUv, 1)).xy, 0, 1);
			}
		", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
		shaderStore.AddShaderPart( DrawableRenderer.TestFragment, new SpirvBytecode( @"#version 450
			layout(location = 0) in vec2 inUv;

			layout(location = 0) out vec4 outColor;

			layout(binding = 1, set = 1) uniform sampler2D texSampler;

			void main () {
				outColor = texture( texSampler, inUv );
			}
		", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

		var image = Image.Load<Rgba32>( "./texture.jpg" );
		image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		texture = new( image );

		drawableRenderer = new( container = new() {
			Position = new( -1 ),
			Scale = new( 2 )
		} );
		container.AddChild( new Sprite( shaderStore, texture ) {
			Scale = new( 200 )
		} );
		for ( int i = 0; i < 10; i++ ) {
			container.AddChild( new Sprite( shaderStore, texture ) {
				Scale = new( 100 * float.Pow( 0.5f, i ) ),
				X = 200,
				Y = 200 - 200 * float.Pow( 0.5f, i )
			} );
		}
	}

	struct GlobalUniforms { // TODO we need a debug check for memory alignment in these
		public Matrix4x3<float> Matrix;
	}
	IHostBuffer<GlobalUniforms> globalUniformBuffer = null!;
	protected override void Initialize () {
		base.Initialize();

		globalUniformBuffer = Renderer.CreateHostBuffer<GlobalUniforms>( BufferType.Uniform );
		globalUniformBuffer.Allocate( 1, BufferUsage.GpuRead | BufferUsage.CpuWrite | BufferUsage.GpuPerFrame | BufferUsage.CpuPerFrame );

		var basic = shaderStore.GetShader( new() { Vertex = DrawableRenderer.TestVertex, Fragment = DrawableRenderer.TestFragment } );
		
		shaderStore.CompileNew( Renderer );
		var globalSet = basic.Value.GetUniformSet( 0 );
		globalSet.SetUniformBuffer( globalUniformBuffer, binding: 0 );
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		shaderStore.CompileNew( commands.Renderer );
		var mat = Matrix3<float>.CreateViewport( 1, 1, Window.Width, Window.Height ) * new Matrix3<float>( commands.Renderer.CreateLeftHandCorrectionMatrix<float>() );
		globalUniformBuffer.Upload( new GlobalUniforms {
			Matrix = new( mat )
		} );

		using var _ = commands.RenderTo( framebuffer );
		commands.SetTopology( Topology.Triangles );
		commands.SetViewport( framebuffer.Size );
		commands.SetScissors( framebuffer.Size );

		drawableRenderer.CollectDrawData();
		drawableRenderer.Draw( commands );
	}

	protected override void Dispose () {
		container.Dispose();
		texture.Dispose();
		shaderStore.Dispose();
	}
}