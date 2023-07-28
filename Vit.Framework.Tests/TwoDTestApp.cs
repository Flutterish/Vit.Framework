using System.Collections.Concurrent;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Input;
using Vit.Framework.Graphics.TwoD.Input.Events;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Graphics.TwoD.UI;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Fonts.OpenType;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;
using Vit.Framework.Windowing.Sdl.Input;

namespace Vit.Framework.Tests;

public class TwoDTestApp : App {
	Type type;
	public TwoDTestApp ( Type type ) : base( "Test App" ) {
		this.type = type;
	}

	Host host = null!;
	Window window = null!;
	DrawableViewportContainer<Drawable> root = null!;
	DrawNodeRenderer drawableRenderer = null!;
	RenderThreadScheduler disposeScheduler = null!;

	UpdateThread updateThread = null!;

	protected override void Initialize () {
		host = new SdlHost( primaryApp: this );
		var api = host.SupportedRenderingApis.First( x => x.KnownName == KnownGraphicsApiName.OpenGl );
		window = host.CreateWindow( api );
		window.Title = $"New Window [{Name}] [{api}] (Testing {type})";
		window.Initialized += _ => {
			root = new( (1920, 1080), window.Size.Cast<float>(), FillMode.Fit ) {
				Position = (-1, -1)
			};

			drawableRenderer = new( root );

			DependencyCache deps = new();

			var shaderStore = new ShaderStore();
			deps.Cache( shaderStore );

			var textureStore = new TextureStore();
			deps.Cache( textureStore );

			var fontStore = new FontStore();
			fontStore.AddFont( FontStore.DefaultFont, new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) ) );
			deps.Cache( fontStore );

			disposeScheduler = new RenderThreadScheduler();
			deps.Cache( disposeScheduler );

			shaderStore.AddShaderPart( DrawNodeRenderer.TestVertex, new SpirvBytecode( @"#version 450
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
			", ShaderLanguage.GLSL, ShaderPartType.Vertex ) );
			shaderStore.AddShaderPart( DrawNodeRenderer.TestFragment, new SpirvBytecode( @"#version 450
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
			", ShaderLanguage.GLSL, ShaderPartType.Fragment ) );

			root.AddChild( new Sprite() {
				Scale = (1920, 1080),
				Tint = ColorRgba.DarkGray
			} );
			var instance = Activator.CreateInstance( type );
			if ( instance is Drawable drawable ) {
				root.AddChild( drawable );
			}
			else if ( instance is UIComponent component ) {
				root.AddChild( component );
			}
			else {
				throw new InvalidOperationException( "the test type is funky" );
			}

			root.TryLoad( deps );

			ThreadRunner.RegisterThread( updateThread = new UpdateThread( drawableRenderer, window, disposeScheduler, $"Update Thread [{Name}]" ) { RateLimit = 240 } );
			ThreadRunner.RegisterThread( new RenderThread( drawableRenderer, host, window, api, disposeScheduler, $"Render Thread [{Name}]" ) { RateLimit = 60 } );
		};

		window.Closed += _ => {
			updateThread.Scheduler.Enqueue( () => {
				root?.DisposeChildren();
				root?.Dispose();
			} );
			Task.Delay( 1000 ).ContinueWith( _ => Quit() );
		};
	}

	public class RenderThread : AppThread {
		DrawNodeRenderer drawableRenderer;
		RenderThreadScheduler disposeScheduler;
		GraphicsApi api;
		Window window;
		public RenderThread ( DrawNodeRenderer drawableRenderer, Host host, Window window, GraphicsApiType api, RenderThreadScheduler disposeScheduler, string name ) : base( name ) {
			this.api = host.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			this.window = window;
			this.drawableRenderer = drawableRenderer;
			this.disposeScheduler = disposeScheduler;

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
			textureStore = ((ICompositeDrawable<Drawable>)drawableRenderer.Root).Dependencies.Resolve<TextureStore>();
			var basic = shaderStore.GetShader( new() { Vertex = DrawNodeRenderer.TestVertex, Fragment = DrawNodeRenderer.TestFragment } );

			shaderStore.CompileNew( renderer );
			var globalSet = basic.Value.GetUniformSet( 0 );
			globalSet.SetUniformBuffer( globalUniformBuffer, binding: 0 );
		}

		ShaderStore shaderStore = null!;
		TextureStore textureStore = null!;
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
			textureStore.UploadNew( renderer );
			var mat = Matrix3<float>.CreateViewport( 1, 1, window.Width / 2, window.Height / 2 ) * new Matrix3<float>( renderer.CreateLeftHandCorrectionMatrix<float>() );
			globalUniformBuffer.Upload( new GlobalUniforms {
				Matrix = new( mat )
			} );
			using ( var commands = swapchain.CreateImmediateCommandBufferForPresentation() ) {
				using var _ = commands.RenderTo( frame, ColorRgba.Blue );
				commands.SetTopology( Topology.Triangles );
				commands.SetViewport( frame.Size );
				commands.SetScissors( frame.Size );

				drawableRenderer.Draw( commands, disposeScheduler.Execute );
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
		Sprite cursor;

		UIEventSource uiEventSource;
		GlobalInputTrackers globalInputTrackers;
		CursorState.Tracker cursorTracker;
		DrawNodeRenderer drawableRenderer;
		RenderThreadScheduler disposeScheduler;
		Window window;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		public UpdateThread ( DrawNodeRenderer drawableRenderer, Window window, RenderThreadScheduler disposeScheduler, string name ) : base( name ) {
			this.drawableRenderer = drawableRenderer;
			this.disposeScheduler = disposeScheduler;
			this.window = window;

			uiEventSource = new() { Root = drawableRenderer.Root };
			globalInputTrackers = new() { Root = drawableRenderer.Root };
			cursorTracker = new CursorTracker( (SdlWindow)window );
			globalInputTrackers.Add( cursorTracker );

			cursor = new CursorSprite {
				Size = new( 18 ),
				Tint = ColorRgba.HotPink
			};
			((DrawableViewportContainer<Drawable>)drawableRenderer.Root).AddChild( cursor );

			globalInputTrackers.EventEmitted += e => {
				var translated = uiEventSource.TriggerEvent( e );

				if ( translated || e is not ILoggableEvent )
					return;

				Console.WriteLine( $"{e} was not translated to a UI event" );
			};
		}

		class CursorSprite : Sprite {
			public CursorSprite () {
				RemoveEventHandler<HoveredEvent>();
			}
		}

		protected override void Dispose ( bool disposing ) {
			globalInputTrackers.Dispose();
		}

		protected override void Initialize () {
			
		}

		protected override void Loop () {
			while ( Scheduler.TryDequeue( out var action ) ) {
				action();
			}

			var root = (DrawableViewportContainer<Drawable>)drawableRenderer.Root;
			root.Size = window.Size.Cast<float>();
			globalInputTrackers.Update();

			var pos = drawableRenderer.Root.ScreenSpaceToLocalSpace( cursorTracker.State.ScreenSpacePosition );
			cursor.Position = pos - new Vector2<float>( 9f );
			cursor.Tint = cursorTracker.State.IsDown( CursorButton.Left )
				? ColorRgba.Red
				: cursorTracker.State.IsDown( CursorButton.Right )
				? ColorRgba.Blue
				: ColorRgba.HotPink;

			foreach ( var i in root.Children.Take(2).OfType<IDrawableLayoutElement>() ) {
				i.Size = root.ContentSize;
			}

			drawableRenderer.Root.Update();

			drawableRenderer.CollectDrawData( disposeScheduler.Swap );
		}
	}
}
