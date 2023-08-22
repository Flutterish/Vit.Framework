using System.Diagnostics;
using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.OpenGl.Windowing;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Interop;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.OpenGl.Queues;

public class GlSwapchain : ISwapchain {
	IGlWindow window;
	nint context;

	public GlSwapchain ( GlRenderer renderer, IGlWindow window ) {
		this.window = window;
		context = window.CreateContext();
		OpenGlApi.loadBindings(); // bindings have to be loaded after a context is created to load all functions
		GL.Enable( EnableCap.FramebufferSrgb ); // TODO this should only be done for the default framebuffer, but the enable cap does it for all framebuffers
		enableDebug();
		immediateCommandBuffer = renderer.CreateImmediateCommandBuffer();
	}

	[Conditional("DEBUG")]
	static void enableDebug () {
		GL.Enable( EnableCap.DebugOutput );
		GL.DebugMessageCallback( static ( source, type, id, severity, length, message, userParam ) => {
			if ( type == DebugType.DebugTypeError ) {
				var msg = new CString( message );
				Debug.Fail( msg );
			}
		}, 0 );
	}

	public Size2<uint> BackbufferSize { get; private set; }
	public void Recreate () {

	}

	DefaultFrameBuffer frameBuffer = new();
	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		window.MakeCurrent( context );

		frameIndex = 0;
		BackbufferSize = window.PixelSize.Cast<uint>();
		return frameBuffer;
	}

	public void Present ( int frameIndex ) {
		window.SwapBackbuffer();
		GL.Finish();
	}

	IImmediateCommandBuffer immediateCommandBuffer;
	public IImmediateCommandBuffer CreateImmediateCommandBufferForPresentation () {
		return immediateCommandBuffer;
	}

	public void Dispose () {
		// TODO should we "destroy" the context?
	}
}
