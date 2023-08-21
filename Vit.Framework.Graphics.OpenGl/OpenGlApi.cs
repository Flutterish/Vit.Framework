﻿using OpenTK;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.Graphics.OpenGl;

public class OpenGlApi : GraphicsApi {
	public static readonly GraphicsApiType GraphicsApiType = new() {
		KnownName = KnownGraphicsApiName.OpenGl,
		Name = "OpenGL Core",
		Version = 460
	};

	static IBindingsContext? bindings;
	static object loadLock = new();
	public OpenGlApi ( IEnumerable<RenderingCapabilities> capabilities ) : base( GraphicsApiType, capabilities ) {
		
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

	public static Dictionary<Graphics.Rendering.Textures.PixelFormat, PixelInternalFormat> internalFormats = new() {
		[Graphics.Rendering.Textures.PixelFormat.Rgb8] = PixelInternalFormat.Rgb8,
		[Graphics.Rendering.Textures.PixelFormat.Rgba8] = PixelInternalFormat.Rgba8,
		[Graphics.Rendering.Textures.PixelFormat.D24] = PixelInternalFormat.DepthComponent24,
		[Graphics.Rendering.Textures.PixelFormat.D32f] = PixelInternalFormat.DepthComponent32f,
		[Graphics.Rendering.Textures.PixelFormat.D24S8ui] = PixelInternalFormat.Depth24Stencil8,
		[Graphics.Rendering.Textures.PixelFormat.D32fS8ui] = PixelInternalFormat.Depth32fStencil8
	};
}
