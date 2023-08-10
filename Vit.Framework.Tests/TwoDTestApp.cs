using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Uniforms;
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
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Templates;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;
using Vit.Framework.Windowing.Sdl.Input;

namespace Vit.Framework.Tests;

public class TwoDTestApp : Basic2DApp<ViewportContainer<UIComponent>> {
	Type type;
	public TwoDTestApp ( Type type ) : base( "Test App" ) {
		this.type = type;
	}

	protected override Host GetHost () {
		return new SdlHost( primaryApp: this );
	}

	protected override GraphicsApiType SelectGraphicsApi ( IEnumerable<GraphicsApiType> available ) {
		return available.First( x => x.KnownName == KnownGraphicsApiName.Vulkan );
	}

	protected override ViewportContainer<UIComponent> CreateRoot () {
		return new() {
			TargetSize = (1920, 1080),
			Padding = new( all: 20 ),
			Position = (-1, -1)
		};
	}

	protected override void PopulateShaderStore ( ShaderStore shaders ) { base.PopulateShaderStore( shaders ); }

	protected override void PopulateTextureStore ( TextureStore textures ) { }

	protected override void PopulateFontStore ( FontStore fonts ) {
		fonts.AddFont( FontStore.DefaultFont, new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) ) );
	}

	protected override void OnInitialized () {
		Window.Title = $"New Window [{Name}] [{GraphicsApiType}] (Testing {type})";

		Root.AddChild( new Box() { Tint = ColorRgba.DarkGray }, new() {
			Size = new( 1f.Relative() )
		} );
		var _instance = Activator.CreateInstance( type );
		UIComponent instance =
			_instance is Drawable drawable ? new Visual { Displayed = drawable }
			: _instance is UIComponent component ? component
			: throw new InvalidOperationException( "the test type is funky" );
		Root.AddChild( instance, new() {
			Size = new( 1f.Relative() )
		} );

		Root.Load( Dependencies );
	}

	protected override UpdateThread CreateUpdateThread () {
		return new TestUpdateThread( DrawNodeRenderer, DisposeScheduler, Window, Dependencies, $"Update Thread [{Name}]" ) { RateLimit = 240 };
	}

	protected override RenderThread CreateRenderThread () {
		return new TestRenderThread( DrawNodeRenderer, DisposeScheduler, Host, Window, GraphicsApiType, Dependencies, $"Render Thread [{Name}]" ) { RateLimit = 60 };
	}

	class TestUpdateThread : UpdateThread {
		Visual<Sprite> cursor;
		UIEventSource uiEventSource;
		GlobalInputTrackers globalInputTrackers;
		CursorState.Tracker cursorTracker;
		Window window;
		public TestUpdateThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, Window window, IReadOnlyDependencyCache dependencies, string name ) : base( drawNodeRenderer, disposeScheduler, dependencies, name ) {
			uiEventSource = new() { Root = Root };
			globalInputTrackers = new();
			cursorTracker = new CursorTracker( (SdlWindow)window );
			globalInputTrackers.Add( cursorTracker );
			this.window = window;

			Root.AddChild( cursor = new Visual<Sprite> { Displayed = new() { Tint = ColorRgba.HotPink } }, new() {
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

		protected override bool Initialize () {
			return true;
		}

		protected override void OnUpdate () {
			if ( Root.IsDisposed )
				return;

			Root.Size = window.Size.Cast<float>();
			globalInputTrackers.Update();

			var pos = Root.ScreenSpaceToLocalSpace( cursorTracker.State.ScreenSpacePosition );
			Root.UpdateLayoutParameters( cursor, x => x with { Anchor = pos - new Vector2<float>( Root.Padding.Left, Root.Padding.Bottom ) } );
			cursor.Displayed.Tint = cursorTracker.State.IsDown( CursorButton.Left )
				? ColorRgba.Red
				: cursorTracker.State.IsDown( CursorButton.Right )
				? ColorRgba.Blue
				: ColorRgba.HotPink;

			Root.Update();
			Root.ComputeLayout();
		}

		protected override void Dispose ( bool disposing ) {
			globalInputTrackers.Dispose();
			base.Dispose( disposing );
		}
	}

	class TestRenderThread : RenderThread {
		public TestRenderThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, Host host, Window window, GraphicsApiType api, IReadOnlyDependencyCache dependencies, string name ) : base( drawNodeRenderer, disposeScheduler, host, window, api, dependencies, name ) {
		}

		struct GlobalUniforms { // TODO we need a debug check for memory alignment in these
			public Matrix4x3<float> Matrix;
		}
		IUniformSet globalSet = null!;
		IHostBuffer<GlobalUniforms> globalUniformBuffer = null!;
		protected override bool Initialize () {
			if ( !base.Initialize() )
				return false;

			globalUniformBuffer = Renderer.CreateHostBuffer<GlobalUniforms>( BufferType.Uniform );
			globalUniformBuffer.Allocate( 1, BufferUsage.CpuWrite | BufferUsage.GpuRead | BufferUsage.CpuPerFrame | BufferUsage.GpuPerFrame );

			var basic = ShaderStore.GetShader( new() { Vertex = BasicVertexShader.Identifier, Fragment = BasicFragmentShader.Identifier } );

			ShaderStore.CompileNew( Renderer );
			globalSet = basic.Value.CreateUniformSet( 0 );
			globalSet.SetUniformBuffer( globalUniformBuffer, binding: 0 );
			basic.Value.SetUniformSet( globalSet );

			return true;
		}

		protected override void BeforeRender () {
			var mat = Matrix3<float>.CreateViewport( 1, 1, Window.Width / 2, Window.Height / 2 ) * new Matrix3<float>( Renderer.CreateLeftHandCorrectionMatrix<float>() );
			globalUniformBuffer.Upload( new GlobalUniforms {
				Matrix = new( mat )
			} );
		}

		protected override void DisposeGraphics ( bool disposing ) {
			globalSet.Dispose();
			globalUniformBuffer.Dispose();
			base.DisposeGraphics( disposing );
		}
	}
}