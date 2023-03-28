using Vit.Framework.Graphics.Rendering.Synchronisation;

namespace Vit.Framework.Graphics.Rendering.Queues;

public interface ICommandBuffer {
	void Reset ();
	void BeginRecodring ();
	void FinishRecodring ();
	void Submit ( IGpuBarrier beginBarrier, IGpuBarrier finishedBarrier, ICpuBarrier renderingFinishedBarrier );

	public void DrawPrimitives ( int count ) => DrawPrimitives( (uint)count );
	void DrawPrimitives ( uint count );
}
