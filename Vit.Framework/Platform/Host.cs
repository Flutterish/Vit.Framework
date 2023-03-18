﻿using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
	public readonly App PrimaryApp;
	public Host ( App primaryApp ) {
		PrimaryApp = primaryApp;
	}

	public Window CreateWindow ( RenderingApi renderingApi, App app ) {
		var window = CreateWindow( renderingApi );
		window.ThreadCreated += app.ThreadRunner.RegisterThread;
		return window;
	}
	public abstract Window CreateWindow ( RenderingApi renderingApi );

	public abstract IEnumerable<RenderingApi> SupportedRenderingApis { get; }

	public bool IsDisposed { get; private set; }
	public void Dispose () {
		if ( IsDisposed )
			return;

		GC.SuppressFinalize( this );
		Dispose( true );
		IsDisposed = true;
	}
	public abstract void Dispose ( bool isDisposing );

	~Host () {
		Dispose( false );
		IsDisposed = true;
	}
}
