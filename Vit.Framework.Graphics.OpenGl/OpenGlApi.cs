﻿using OpenTK;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.OpenGl;

public class OpenGlApi : GraphicsApi {
	static IBindingsContext? bindings;
	static object loadLock = new();
	public OpenGlApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType.OpenGl, capabilities ) {
		
	}

	internal static void loadBindings () {
		lock ( loadLock ) {
			if ( bindings == null ) {
				InitializeGlBindings();
			}
		}
	}

	private static void InitializeGlBindings () {
		bindings = new WglBindingsContext();

		OpenTK.Graphics.ES11.GL.LoadBindings( bindings );
		OpenTK.Graphics.ES20.GL.LoadBindings( bindings );
		OpenTK.Graphics.ES30.GL.LoadBindings( bindings );
		OpenTK.Graphics.OpenGL.GL.LoadBindings( bindings );
		OpenTK.Graphics.OpenGL4.GL.LoadBindings( bindings );
	}

	protected override void Dispose ( bool disposing ) {
		
	}
}