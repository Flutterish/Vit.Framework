﻿using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Platform;
using Vit.Framework.Windowing;

namespace Vit.Framework.Tests.GraphicsApis;

public class Test00_ClearScreen : GenericRenderThread {
	public Test00_ClearScreen ( Window window, Host host, string name, GraphicsApi api ) : base( window, host, name, api ) {
	}

	protected override void Render ( IFramebuffer framebuffer, ICommandBuffer commands ) {
		using var _ = commands.RenderTo( framebuffer );
		commands.SetClearColor( ColorSRgba.HotPink );
		commands.Clear( ClearFlags.Color );
	}
}
