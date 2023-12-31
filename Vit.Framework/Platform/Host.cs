﻿using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Input;
using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
	public readonly App PrimaryApp;
	public Host ( App primaryApp ) {
		PrimaryApp = primaryApp;
	}

	public abstract Task<Window> CreateWindow ();
	public abstract Clipboard GetClipboard ();
	public abstract GraphicsApi CreateGraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities );

	public abstract IEnumerable<GraphicsApiType> SupportedRenderingApis { get; }

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
