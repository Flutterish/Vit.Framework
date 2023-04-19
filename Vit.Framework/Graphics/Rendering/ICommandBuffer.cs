using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Rendering;

/// <summary>
/// An interface with which you can send commands to the GPU.
/// </summary>
public interface ICommandBuffer {
	/// <summary>
	/// Starts rendering to a frame buffer.
	/// </summary>
	DisposeAction<ICommandBuffer> RenderTo ( IFramebuffer framebuffer, Color4<float>? clearColor = null, float? clearDepth = null, uint? clearStencil = null );
}
