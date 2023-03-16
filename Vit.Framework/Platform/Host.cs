using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
	public abstract Window CreateWindow ();
	public abstract void Dispose ();
}
