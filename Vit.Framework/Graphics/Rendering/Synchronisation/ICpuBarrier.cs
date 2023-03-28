namespace Vit.Framework.Graphics.Rendering.Synchronisation;

public interface ICpuBarrier : IDisposable {
	void Wait ();
	void Reset ();
}
