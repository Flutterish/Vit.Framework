using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
	List<AppThread> appThreads = new();
	public void RegisterThread ( AppThread thread ) {
		appThreads.Add( thread );
	}

	public abstract Window CreateWindow ( RenderingApi renderingApi );

	public abstract IEnumerable<RenderingApi> SupportedRenderingApis { get; }

	public void Quit () {
		Dispose();
	}
	public abstract void Dispose ();
}
