using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Platform;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class TwoDTestApp : App {
	public TwoDTestApp ( string name ) : base( name ) {
		
	}

	Host host = null!;
	Window window = null!;
	ViewportContainer<Drawable> root = null!;
	DrawableRenderer drawableRenderer = null!;
	protected override void Initialize () {
		host = new SdlHost( primaryApp: this );
		var api = host.SupportedRenderingApis.First( x => x.KnownName == KnownGraphicsApiName.OpenGl );
		window = host.CreateWindow( api );
		window.Title = $"New Window [{Name}] [{api}]";
		window.Initialized += _ => {
			root = new( (1920, 1080), window.Size.Cast<float>(), FillMode.Fit ) {
				Position = (-1, -1)
			};

			drawableRenderer = new( root );

			root.TryLoad();
			var deps = (IDependencyCache)root.Dependencies;
			var shaderStore = new ShaderStore();
			deps.Cache( shaderStore );

			var image = Image.Load<Rgba32>( "./texture.jpg" );
			image.Mutate( x => x.Flip( FlipMode.Vertical ) );
			var sampleTexture = new Texture( image );
			deps.Cache( sampleTexture );

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

			root.AddChild( new Sprite() {
				Scale = (1920, 1080)
			} );
			root.AddChild( new Sprite() {
				Scale = (1080, 1080)
			} );
			for ( int i = 0; i < 10; i++ ) {
				root.AddChild( new Sprite() {
					Scale = new( 1080 / 2 * float.Pow( 0.5f, i ) ),
					X = 1080,
					Y = 1080 - 1080 * float.Pow( 0.5f, i )
				} );
			}

			ThreadRunner.RegisterThread( new UpdateThread( drawableRenderer, window, $"Update Thread [{Name}]" ) { RateLimit = 240 } );
			ThreadRunner.RegisterThread( new RenderThread( drawableRenderer, host, window, api, $"Render Thread [{Name}]" ) { RateLimit = 60 } );
		};

		window.Closed += _ => {
			Quit();
		};
	}

	public class RenderThread : AppThread {
		DrawableRenderer drawableRenderer;
		GraphicsApi api;
		Window window;
		public RenderThread ( DrawableRenderer drawableRenderer, Host host, Window window, GraphicsApiType api, string name ) : base( name ) {
			this.api = host.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			this.window = window;
			this.drawableRenderer = drawableRenderer;

			window.Resized += onWindowResized;
		}

		ISwapchain swapchain = null!;
		IRenderer renderer = null!;
		protected override void Initialize () {
			(swapchain, renderer) = window.CreateSwapchain( api, new() { 
				Depth = DepthFormat.Bits24, 
				Stencil = StencilFormat.Bits8,
				Multisample = MultisampleFormat.Samples4
			} );

			globalUniformBuffer = renderer.CreateHostBuffer<GlobalUniforms>( BufferType.Uniform );
			globalUniformBuffer.Allocate( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.CpuPerFrame | BufferUsage.GpuPerFrame );

			shaderStore = ((ICompositeDrawable<Drawable>)drawableRenderer.Root).Dependencies.Resolve<ShaderStore>();
			var basic = shaderStore.GetShader( new() { Vertex = DrawableRenderer.TestVertex, Fragment = DrawableRenderer.TestFragment } );

			shaderStore.CompileNew( renderer );
			var globalSet = basic.Value.GetUniformSet( 0 );
			globalSet.SetUniformBuffer( globalUniformBuffer, binding: 0 );
		}

		ShaderStore shaderStore = null!;
		bool windowResized;
		void onWindowResized ( Window _ ) {
			windowResized = true;
		}

		struct GlobalUniforms { // TODO we need a debug check for memory alignment in these
			public Matrix4x3<float> Matrix;
		}
		IHostBuffer<GlobalUniforms> globalUniformBuffer = null!;
		protected override void Loop () {
			if ( windowResized ) { // BUG this can crash and is laggy
				windowResized = false;
				swapchain.Recreate();
			}

			if ( swapchain.GetNextFrame( out var index ) is not IFramebuffer frame )
				return;

			shaderStore.CompileNew( renderer );
			var mat = Matrix3<float>.CreateViewport( 1, 1, window.Width / 2, window.Height / 2 ) * new Matrix3<float>( renderer.CreateLeftHandCorrectionMatrix<float>() );
			globalUniformBuffer.Upload( new GlobalUniforms {
				Matrix = new( mat )
			} );
			using ( var commands = swapchain.CreateImmediateCommandBufferForPresentation() ) {
				using var _ = commands.RenderTo( frame );
				commands.SetTopology( Topology.Triangles );
				commands.SetViewport( frame.Size );
				commands.SetScissors( frame.Size );

				drawableRenderer.Draw( commands );
			}
			swapchain.Present( index );
		}

		protected override void Dispose ( bool disposing ) {
			window.Resized -= onWindowResized;

			if ( !IsInitialized )
				return;

			renderer.WaitIdle();

			swapchain.Dispose();
			renderer.Dispose();
			api.Dispose();
		}
	}

	public class UpdateThread : AppThread {
		DrawableRenderer drawableRenderer;
		Window window;
		public UpdateThread ( DrawableRenderer drawableRenderer, Window window, string name ) : base( name ) {
			this.drawableRenderer = drawableRenderer;
			this.window = window;
		}

		protected override void Initialize () {
			drawableRenderer.Root.TryLoad();
		}

		protected override void Loop () {
			((ViewportContainer<Drawable>)drawableRenderer.Root).AvailableSize = window.Size.Cast<float>();
			drawableRenderer.CollectDrawData();
		}
	}
}
