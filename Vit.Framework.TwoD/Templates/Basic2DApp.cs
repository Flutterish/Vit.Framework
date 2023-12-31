﻿using System.Diagnostics;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Pooling;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Localisation;
using Vit.Framework.Memory;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.Timing;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Graphics.Text;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;
using Vit.Framework.TwoD.Rendering.Shaders;
using Vit.Framework.TwoD.UI.Components;
using Vit.Framework.Windowing;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp : App {
	public Basic2DApp ( string name ) : base( name ) { }

	protected Host Host = null!;
	protected Window Window = null!;
	protected DrawNodeRenderer DrawNodeRenderer = null!;
	protected RenderThreadScheduler DisposeScheduler = null!;
	protected DependencyCache Dependencies = new();

	protected GraphicsApiType GraphicsApiType = null!;
	protected UpdateThread MainUpdateThread = null!;
	protected RenderThread MainRenderThread = null!;

	protected IHasDrawNodes<DrawNode> Root = null!;

	protected abstract Host GetHost ();
	protected abstract GraphicsApiType SelectGraphicsApi ( IEnumerable<GraphicsApiType> available );
	protected abstract IHasDrawNodes<DrawNode> CreateRoot ();

	protected virtual void PopulateShaderStore ( ShaderStore shaders ) {
		shaders.AddShaderPart( BasicVertex.Identifier, BasicVertex.Spirv );
		shaders.AddShaderPart( BasicFragment.Identifier, BasicFragment.Spirv );
		shaders.AddShaderPart( MaskedVertex.Identifier, MaskedVertex.Spirv );
		shaders.AddShaderPart( MaskedFragment.Identifier, MaskedFragment.Spirv );
		shaders.AddShaderPart( SvgVertex.Identifier, SvgVertex.Spirv );
		shaders.AddShaderPart( SvgFragment.Identifier, SvgFragment.Spirv );
		shaders.AddShaderPart( TextVertex.Identifier, TextVertex.Spirv );
		shaders.AddShaderPart( TextFragment.Identifier, TextFragment.Spirv );
	}
	protected abstract void PopulateTextureStore ( TextureStore textures );
	protected abstract void PopulateFontStore ( FontStore fonts );

	protected abstract void OnInitialized ();

	protected abstract UpdateThread CreateUpdateThread ();
	protected abstract RenderThread CreateRenderThread ();

	protected sealed override async void Initialize () {
		Host = GetHost();
		GraphicsApiType = SelectGraphicsApi( Host.SupportedRenderingApis );
		Window = await Host.CreateWindow();

		Root = CreateRoot();

		DrawNodeRenderer = new( Root );

		var shaderStore = new ShaderStore();
		PopulateShaderStore( shaderStore );
		Dependencies.Cache( shaderStore );

		var textureStore = new TextureStore();
		PopulateTextureStore( textureStore );
		Dependencies.Cache( textureStore );

		var fontStore = new FontStore();
		PopulateFontStore( fontStore );
		Dependencies.Cache( fontStore );

		DisposeScheduler = new RenderThreadScheduler();
		Dependencies.Cache( DisposeScheduler );

		Dependencies.Cache( new SingleUseBufferSectionStack( 1024 * 1024 * 8 ) ); // 8MiB buffer
		Dependencies.Cache( new DeviceBufferHeap( 1024 * 1024 * 64, 1024 * 16 ) ); // 64MiB buffer, expect 16kiB allocs
		Dependencies.Cache( new MaskingDataBuffer( 1024 * 1024 ) ); // 1Mi * 16B ~ 1Mi instructions or 209ki masking params
		Dependencies.Cache( new TexturedQuad.DrawDependencies() );
		Dependencies.Cache( new DrawableSpriteText.DrawDependencies() );

		StopwatchClock clock = new();
		Dependencies.Cache( clock );
		Dependencies.Cache<IClock>( clock );
		Dependencies.Cache( Host.GetClipboard() );

		MainUpdateThread = CreateUpdateThread();
		MainRenderThread = CreateRenderThread();
		Dependencies.Cache<UpdateThread>( MainUpdateThread );
		Dependencies.Cache<RenderThread>( MainRenderThread );

		Dependencies.Cache( new FpsCounter.FpsCounterData(
			("TPS", MainUpdateThread.UpdateCounter),
			("FPS", MainRenderThread.UpdateCounter)
		) );

		Dependencies.Cache( new StencilFontStore() );
		Dependencies.Cache( new SpriteFontStore( pageSize: (16, 8), glyphSize: (32, 64), shaderStore ) );
		Dependencies.Cache( new LocalisationStore() );

		OnInitialized();
		ThreadRunner.RegisterThread( MainUpdateThread );
		ThreadRunner.RegisterThread( MainRenderThread );

		Window.Closed += _ => {
			MainUpdateThread.Scheduler.Enqueue( () => {
				MainUpdateThread.Renderer = null;
				MainUpdateThread.IsUpdatingActive = false;

				MainRenderThread.Scheduler.Enqueue( () => {
					MainRenderThread.IsRenderingEnabled = false;
					MainRenderThread.Renderer.WaitIdle();

					Root.DisposeDrawNodes();

					Quit();
				} );
			} );
		};
	}

	protected Task SwitchBackend () {
		TaskCompletionSource taskSource = new();
		MainUpdateThread.Scheduler.Enqueue( stopUpdating );
		return taskSource.Task;

		void stopUpdating () {
			MainUpdateThread.Renderer = null;
			MainUpdateThread.IsUpdatingActive = false;

			MainRenderThread.Scheduler.Enqueue( stopRendering );
		}

		void stopRendering () {
			MainRenderThread.IsRenderingEnabled = false;
			disposeGraphics();
		}

		void disposeGraphics () {
			MainRenderThread.Renderer.WaitIdle();

			Root.DisposeDrawNodes();
			MainRenderThread.DisposeRenderer();

			performGcCheck();
			swapBackend();
		}

		void swapBackend () {
			GraphicsApiType = SelectGraphicsApi( Host.SupportedRenderingApis );
			MainRenderThread.ChangeApi( GraphicsApiType );
			initializeGraphics();
		}

		void initializeGraphics () {
			if ( !MainRenderThread.InitializeGraphics() ) {
				MainRenderThread.Scheduler.Enqueue( initializeGraphics );
			}
			else {
				MainRenderThread.IsRenderingEnabled = true;
				MainUpdateThread.Scheduler.Enqueue( resumeUpdating );
			}
		}

		void resumeUpdating () {
			MainUpdateThread.IsUpdatingActive = true;
			taskSource.SetResult();
		}

		[Conditional("DEBUG")]
		static void performGcCheck () {
			Console.WriteLine( "Performing GC check before renderer switch..." );
			DisposableObject.ValidateEverythingIsDisposed();
		}
	}
}
