using Vit.Framework.Graphics.Rendering.Synchronisation;
using Vit.Framework.Graphics.Rendering.Textures;

namespace Vit.Framework.Graphics.Rendering.Queues;

public interface ISwapchain : IDisposable {
	int FrameBufferCount { get; }
	(IFrameBuffer frameBuffer, int index) GetNextFrameBuffer ( IGpuBarrier frameAvailableBarrier, ICpuBarrier? cpuBarrier = null );

	IQueue GraphicsQueue { get; }
	IQueue PresentQueue { get; }
}