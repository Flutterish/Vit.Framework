﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.Windowing;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp<TRoot> : App where TRoot : class, IHasDrawNodes<DrawNode> {
	public Basic2DApp ( string name ) : base( name ) { }

	protected Host Host = null!;
	protected Window Window = null!;
	protected DrawNodeRenderer DrawNodeRenderer = null!;
	protected RenderThreadScheduler DisposeScheduler = null!;
	protected DependencyCache Dependencies = new();

	protected GraphicsApiType GraphicsApiType = null!;
	protected UpdateThread MainUpdateThread = null!;
	protected RenderThread MainRenderThread = null!;

	protected TRoot Root = null!;

	protected abstract Host GetHost ();
	protected abstract GraphicsApiType SelectGraphicsApi ( IEnumerable<GraphicsApiType> available );
	protected abstract TRoot CreateRoot ();

	protected abstract void PopulateShaderStore ( ShaderStore shaders );
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

		OnInitialized();

		ThreadRunner.RegisterThread( MainUpdateThread = CreateUpdateThread() );
		ThreadRunner.RegisterThread( MainRenderThread = CreateRenderThread() );

		Window.Closed += _ => {
			MainUpdateThread.Scheduler.Enqueue( () => {
				if ( Root is IDisposable disposableRoot )
					disposableRoot.Dispose();

				foreach ( var (id, dep) in Dependencies.EnumerateCached() ) {
					if ( dep is IDisposable disposable ) {
						DisposeScheduler.ScheduleDisposal( disposable );
					}
				}

				Task.Delay( 1000 ).ContinueWith( _ => Quit() );
			} );
		};
	}
}
