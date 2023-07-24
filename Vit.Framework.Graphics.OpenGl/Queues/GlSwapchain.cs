using Vit.Framework.Graphics.OpenGl.Textures;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Queues;
using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.OpenGl.Queues;

public class GlSwapchain : ISwapchain {
	IGlWindow window;
	nint context;

	public GlSwapchain ( GlRenderer renderer, IGlWindow window ) {
		this.window = window;
		context = window.CreateContext();
		OpenGlApi.loadBindings(); // bindings have to be loaded after a context is created to load all functions
		immediateCommandBuffer = renderer.CreateImmediateCommandBuffer();
	}

	public void Recreate () {
		
	}

	DefaultFrameBuffer frameBuffer = new();
	public IFramebuffer? GetNextFrame ( out int frameIndex ) {
		window.MakeCurrent( context );

		frameIndex = 0;
		frameBuffer.Size = window.PixelSize.Cast<uint>();
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
