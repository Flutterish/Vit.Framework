using Vit.Framework.Threading;
using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
	List<AppThread> appThreads = new();
	protected void RegisterThread ( AppThread thread ) {
		appThreads.Add( thread );
	}

	public abstract Window CreateWindow ();
	
	public void Quit () {
		Dispose();
	}
	public abstract void Dispose ();
}
