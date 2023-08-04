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
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Fonts.OpenType;
using Vit.Framework.Threading;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;
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
	ViewportContainer<UIComponent> root = null!;
	DrawNodeRenderer drawNodeRenderer = null!;
	RenderThreadScheduler disposeScheduler = null!;
	DependencyCache dependencies = new();

	UpdateThread updateThread = null!;

	protected override void Initialize () {
		host = new SdlHost( primaryApp: this );
		var api = host.SupportedRenderingApis.First( x => x.KnownName == KnownGraphicsApiName.OpenGl );
		window = host.CreateWindow( api );
		window.Title = $"New Window [{Name}] [{api}] (Testing {type})";
		window.Initialized += _ => {
			root = new() {
				TargetSize = (1920, 1080),
				Padding = new( all: 20 ),
				Position = (-1, -1)
			};

			drawNodeRenderer = new( root );

			var shaderStore = new ShaderStore();
			dependencies.Cache( shaderStore );

			var textureStore = new TextureStore();
			dependencies.Cache( textureStore );

			var fontStore = new FontStore();
			fontStore.AddFont( FontStore.DefaultFont, new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) ) );
			dependencies.Cache( fontStore );

			disposeScheduler = new RenderThreadScheduler();
			dependencies.Cache( disposeScheduler );

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

			root.AddChild( new Box() { Tint = ColorRgba.DarkGray }, new() { 
				Size = new( 1f.Relative() ) 
			} );
			var _instance = Activator.CreateInstance( type );
			UIComponent instance = 
				_instance is Drawable drawable ? new Visual { Displayed = drawable }
				: _instance is UIComponent component ? component 
				: throw new InvalidOperationException( "the test type is funky" );
			root.AddChild( instance, new() {
				Size = new( 1f.Relative() )
			} );

			root.Load( dependencies );

			ThreadRunner.RegisterThread( updateThread = new UpdateThread( drawNodeRenderer, window, disposeScheduler, $"Update Thread [{Name}]" ) { RateLimit = 240 } );
			ThreadRunner.RegisterThread( new RenderThread( drawNodeRenderer, host, window, api, disposeScheduler, $"Render Thread [{Name}]" ) { RateLimit = 60 } );
		};

		window.Closed += _ => {
			updateThread.Scheduler.Enqueue( () => {
				root?.ClearChildren( dispose: true );
				root?.Dispose();

				foreach ( var (id, dep) in dependencies.EnumerateCached() ) {
					if ( dep is IDisposable disposable ) {
						disposeScheduler.ScheduleDisposal( disposable );
					}
				}

				Task.Delay( 1000 ).ContinueWith( _ => Quit() );
			} );
		};
	}

	public class RenderThread : AppThread {
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		GraphicsApi api;
		Window window;
		public RenderThread ( DrawNodeRenderer drawNodeRenderer, Host host, Window window, GraphicsApiType api, RenderThreadScheduler disposeScheduler, string name ) : base( name ) {
			this.api = host.CreateGraphicsApi( api, new[] { RenderingCapabilities.DrawToWindow } );
			this.window = window;
			this.drawNodeRenderer = drawNodeRenderer;
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

			var root = (ICompositeUIComponent<UIComponent>)drawNodeRenderer.Root;
			shaderStore = root.Dependencies.Resolve<ShaderStore>();
			textureStore = root.Dependencies.Resolve<TextureStore>();
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

				drawNodeRenderer.Draw( commands, disposeScheduler.Execute );
			}
			swapchain.Present( index );
		}

		protected override void Dispose ( bool disposing ) {
			window.Resized -= onWindowResized;

			if ( !IsInitialized )
				return;

			renderer.WaitIdle();

			disposeScheduler.DisposeAll();

			globalUniformBuffer?.Dispose();

			swapchain.Dispose();
			renderer.Dispose();
			api.Dispose();
		}
	}

	public class UpdateThread : AppThread {
		Visual<Sprite> cursor;

		UIEventSource uiEventSource;
		GlobalInputTrackers globalInputTrackers;
		CursorState.Tracker cursorTracker;
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		Window window;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		public UpdateThread ( DrawNodeRenderer drawNodeRenderer, Window window, RenderThreadScheduler disposeScheduler, string name ) : base( name ) {
			this.drawNodeRenderer = drawNodeRenderer;
			this.disposeScheduler = disposeScheduler;
			this.window = window;

			var root = (ViewportContainer<UIComponent>)drawNodeRenderer.Root;
			uiEventSource = new() { Root = root };
			globalInputTrackers = new();
			cursorTracker = new CursorTracker( (SdlWindow)window );
			globalInputTrackers.Add( cursorTracker );

			root.AddChild( cursor = new Visual<Sprite> { Displayed = new() { Tint = ColorRgba.HotPink } }, new() { 
				Size = new( 18 ),
				Origin = Anchor.Centre
			} );

			globalInputTrackers.EventEmitted += e => {
				var translated = uiEventSource.TriggerEvent( e );

				if ( translated || e is not ILoggableEvent )
					return;

				Console.WriteLine( $"{e} was not translated to a UI event" );
			};
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

			var root = (ViewportContainer<UIComponent>)drawNodeRenderer.Root;
			if ( root.IsDisposed )
				return;

			root.Size = window.Size.Cast<float>();
			globalInputTrackers.Update();

			var pos = root.ScreenSpaceToLocalSpace( cursorTracker.State.ScreenSpacePosition );
			root.UpdateLayoutParameters( cursor, x => x with { Anchor = pos - new Vector2<float>(root.Padding.Left, root.Padding.Bottom) } );
			cursor.Displayed.Tint = cursorTracker.State.IsDown( CursorButton.Left )
				? ColorRgba.Red
				: cursorTracker.State.IsDown( CursorButton.Right )
				? ColorRgba.Blue
				: ColorRgba.HotPink;

			root.Update();
			root.ComputeLayout();

			drawNodeRenderer.CollectDrawData( disposeScheduler.Swap );
		}
	}
}
