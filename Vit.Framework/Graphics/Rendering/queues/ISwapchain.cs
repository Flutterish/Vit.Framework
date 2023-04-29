using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.Rendering.Queues;

/// <summary>
/// A managed swapchain of window framebuffers.
/// </summary>
/// <remarks>
/// This is equivalent to <c>VkSwapchain</c>, single/double buffering in OpenGL or <c>ID3DXSwapchain</c>.
/// </remarks>
public interface ISwapchain : IDisposable {
	/// <summary>
	/// Recreates the swapchain by disposing old resources and creating new ones, matching current optimal parameters.
	/// </summary>
	void Recreate ();

	/// <summary>
	/// Retreives the framebuffer for the next frame, potentially idling until the frame is available.
	/// </summary>
	/// <param name="frameIndex">The index of the frame.</param>
	/// <returns>The framebuffer, or <see langword="null"/> if the swapchain needs recreation.</returns>
	IFramebuffer? GetNextFrame ( out int frameIndex );

	/// <summary>
	/// Pushes the frame to the swapchain queue. The swapchain might recreate itself at this point if it is not optimal.
	/// </summary>
	/// <param name="frameIndex">Frame index obtained from <see cref="GetNextFrame(out int)"/>.</param>
	void Present ( int frameIndex );

	/// <summary>
	/// Creates an <see cref="IImmediateCommandBuffer"/> which will be used to render a frame.
	/// </summary>
	/// <remarks>
	/// This command buffer will synchronise itself to execute when a swapchain image is ready,
	/// and will signal the swapchain to present when it finishes rendering.
	/// </remarks>
	IImmediateCommandBuffer CreateImmediateCommandBufferForPresentation (); // TODO figure out synchronisation objects instead
}

public struct SwapChainArgs {
	public AcceptableRange<MultisampleFormat> Multisample;
	public AcceptableRange<DepthFormat> Depth;
	public AcceptableRange<StencilFormat> Stencil;
}
