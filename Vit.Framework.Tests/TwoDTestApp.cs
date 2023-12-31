﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Buffers;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Input;
using Vit.Framework.Input.Events;
using Vit.Framework.Input.Trackers;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Text.Fonts.OpenType;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.Templates;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Components;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input.Events.EventSources;
using Vit.Framework.TwoD.UI.Layout;
using Vit.Framework.Windowing;
using Vit.Framework.Windowing.Sdl;

namespace Vit.Framework.Tests;

public class TwoDTestApp : Basic2DApp {
	Type type;
	public TwoDTestApp ( Type type ) : base( "Test App" ) {
		this.type = type;
	}

	KnownGraphicsApiName targetBackend = KnownGraphicsApiName.Vulkan;
	public void SwitchBackends ( KnownGraphicsApiName backend ) {
		targetBackend = backend;

		Window.Title = $"New Window [{Name}] [Switching to: {backend}] (Testing {type})";
		SwitchBackend().ContinueWith( _ => {
			Window.Title = $"New Window [{Name}] [{GraphicsApiType}] (Testing {type})";
		} ); // TODO ensure you cant do this again while its switching already
	}

	protected override Host GetHost () {
		return new SdlHost( primaryApp: this );
	}

	protected override GraphicsApiType SelectGraphicsApi ( IEnumerable<GraphicsApiType> available ) {
		return available.First( x => x.KnownName == targetBackend );
	}

	new protected ViewportContainer<UIComponent> Root => (ViewportContainer<UIComponent>)base.Root;
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
		var consolas = new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) );
		var twemoji = new OpenTypeFont( new ReopenableFileStream( "./Twemoji.ttf" ) );
		fonts.AddFont( FontStore.DefaultFont, consolas );
		fonts.AddFont( FontStore.DefaultEmojiFont, twemoji );
		fonts.AddFont( FrameworkUIScheme.Font, consolas );
		fonts.AddFont( FrameworkUIScheme.EmojiFont, twemoji );
	}

	protected override void OnInitialized () {
		Window.Title = $"New Window [{Name}] [{GraphicsApiType}] (Testing {type})";

		Dependencies.Cache( this );

		Root.AddChild( new Box() { Tint = ColorRgb.DarkGray }, new() {
			Size = new( 1f.Relative() )
		} );
		var _instance = Activator.CreateInstance( type );
		UIComponent instance =
			_instance is Drawable drawable ? new Visual( drawable )
			: _instance is UIComponent component ? component
			: throw new InvalidOperationException( "the test type is funky" );
		Root.AddChild( instance, new() {
			Size = new( 1f.Relative() )
		} );
		Root.AddChild( new FpsCounter(), new() {
			Origin = Anchor.BottomLeft,
			Anchor = Anchor.BottomLeft
		} );

		Root.AddChild( new DrawVisualizer( Root ), new() {
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
		Dictionary<CursorState.Tracker, Visual<Sprite>> cursors = new();

		UIEventSource uiEventSource;
		Window window;
		InputTrackerCollection inputTrackers;
		new protected ViewportContainer<UIComponent> Root => (ViewportContainer<UIComponent>)base.Root;
		public TestUpdateThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, Window window, IReadOnlyDependencyCache dependencies, string name ) : base( drawNodeRenderer, disposeScheduler, dependencies, name ) {
			inputTrackers = window.CreateInputTrackers();
			inputTrackers.BindTrackerState( detected: onTrackerDetected, lost: onTrackerLost );
			uiEventSource = new( Root, dependencies );
			this.window = window;
		}

		void onTrackerDetected ( IInputTracker detected ) {
			if ( detected is CursorState.Tracker cursor ) {
				var visual = new Visual<Sprite>( new() { Tint = ColorRgb.HotPink } );
				cursors.Add( cursor, visual );
				Root.AddChild( visual, new() {
					Size = new( 18 ),
					Origin = Anchor.Centre
				} );

				cursor.InputEventEmitted += onCursorInputEventEmitted;
			}
			else {
				detected.InputEventEmitted += onInputEventEmitted;
			}
		}

		void onTrackerLost ( IInputTracker lost ) {
			if ( lost is CursorState.Tracker cursor ) {
				cursors.Remove( cursor, out var visual );
				Root.RemoveChild( visual! );

				cursor.InputEventEmitted -= onCursorInputEventEmitted;
			}
			else {
				lost.InputEventEmitted -= onInputEventEmitted;
			}
		}

		protected override bool Initialize () {
			Root.AddChild( uiEventSource.TabVisualizer, new() {
				Size = new( 1f.Relative() )
			} );
			return true;
		}

		protected override void OnUpdate () {
			if ( !Root.IsLoaded )
				return;

			inputTrackers.Update();

			Root.Size = window.Size.Cast<float>();
			foreach ( var i in inputTrackers.Trackers ) {
				i.Update();
			}

			Root.Update();
			Root.ComputeLayout();
			Root.UpdateMasking( new AxisAlignedBox2<float>( (-1, -1), window.Size.Cast<float>() ) );
		}

		void onInputEventEmitted ( IInputTracker src, TimestampedEvent e ) {
			var translated = uiEventSource.TriggerEvent( e );

			if ( translated || e is not ILoggableEvent )
				return;

			Console.WriteLine( $"{e} was not translated to a UI event" );
		}

		private void onCursorInputEventEmitted ( IInputTracker src, TimestampedEvent e ) {
			var cursorTracker = (CursorState.Tracker)src;
			var cursor = cursors[cursorTracker];

			var pos = Root.ScreenSpaceToLocalSpace( cursorTracker.State.ScreenSpacePosition );
			Root.UpdateLayoutParameters( cursor, x => x with { Anchor = pos - new Vector2<float>( Root.Padding.Left, Root.Padding.Bottom ) } );
			cursor.Displayed.Tint = cursorTracker.State.IsDown( CursorButton.Left )
				? ColorRgb.Red
				: cursorTracker.State.IsDown( CursorButton.Right )
				? ColorRgb.Blue
				: ColorRgb.HotPink;

			onInputEventEmitted( src, e );
		}

		protected override void Dispose ( bool disposing ) {
			inputTrackers.Dispose();
			Root.Unload();
			base.Dispose( disposing );
		}
	}

	class TestRenderThread : RenderThread {
		public TestRenderThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, Host host, Window window, GraphicsApiType api, IReadOnlyDependencyCache dependencies, string name ) : base( drawNodeRenderer, disposeScheduler, host, window, api, dependencies, name ) {
		}

		struct GlobalUniforms {
			public Matrix4x3<float> Matrix;
			public Size2<uint> ScreenSize;
		}
		IUniformSet globalSet = null!;
		IUniformSetPool globalSetPool = null!;
		IHostBuffer<GlobalUniforms> globalUniformBuffer = null!;
		public override bool InitializeGraphics () {
			if ( !base.InitializeGraphics() )
				return false;

			globalUniformBuffer = Renderer.CreateUniformHostBuffer<GlobalUniforms>( 1, BufferType.Uniform, BufferUsage.CpuWrite );

			var basic = ShaderStore.GetShader( new() {
				Vertex = BasicVertex.Description,
				Fragment = BasicFragment.Identifier
			} );
			var masked = ShaderStore.GetShader( new() {
				Vertex = MaskedVertex.Description,
				Fragment = MaskedFragment.Identifier
			} );
			var text = ShaderStore.GetShader( new() {
				Vertex = TextVertex.Description,
				Fragment = TextFragment.Identifier
			} );

			ShaderStore.CompileNew( Renderer );
			(globalSet, globalSetPool) = masked.Value.CreateUniformSet( 0 );
			globalSet.SetUniformBuffer( globalUniformBuffer, binding: 0 );
			globalSet.SetStorageBufferRaw( MaskingData.StorageBuffer, binding: 1, size: MaskingData.ByteSize );
			globalSet.SetStorageBufferRaw( MaskingData.StorageBuffer, binding: 2, size: MaskingData.ByteSize );

			basic.Value.SetUniformSet( globalSet );
			text.Value.SetUniformSet( globalSet );
			masked.Value.SetUniformSet( globalSet );

			return true;
		}

		protected override void BeforeRender () {
			var mat = Matrix3<float>.CreateViewport( 1, 1, Window.Width / 2, Window.Height / 2 ) * new Matrix3<float>( Renderer.CreateNdcCorrectionMatrix<float>() );
			globalUniformBuffer.UploadUniform( new GlobalUniforms {
				Matrix = new( mat ),
				ScreenSize = Window.Size
			} );
		}

		protected override void DisposeGraphics () {
			globalSetPool.Dispose();
			globalUniformBuffer.Dispose();
			base.DisposeGraphics();
		}
	}
}