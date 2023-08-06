﻿using Vit.Framework.Graphics.Curses;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Platform;

namespace Vit.Framework.Windowing.Console;

public class ConsoleHost : Host {
	public ConsoleHost ( App primaryApp ) : base( primaryApp ) {
		
	}

	public override Task<Window> CreateWindow () {
		return Task.FromResult<Window>( new ConsoleWindow() );
	}

	public override GraphicsApi CreateGraphicsApi ( GraphicsApiType api, IEnumerable<RenderingCapabilities> capabilities ) {
		return api switch {
			var x when x == CursesApi.GraphicsApiType => new CursesApi( capabilities ),
			_ => throw new ArgumentException( $"Unsupported rendering api: {api}", nameof( api ) )
		};
	}

	public override IEnumerable<GraphicsApiType> SupportedRenderingApis { get; } = new[] {
		CursesApi.GraphicsApiType
	};

	public override void Dispose ( bool isDisposing ) {
		throw new NotImplementedException();
	}
}
