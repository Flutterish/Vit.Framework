namespace Vit.Framework.Threading;

public abstract class AppThread {
	public readonly string Name;
	Thread? nativeThread;

	public AppThread ( string name ) {
		Name = name;
		nativeThread = new Thread( onThreadStart ) { Name = name };
		nativeThread.Start();
	}

	bool isInitialized;
	void onThreadStart () {
		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		while ( true ) {
			Loop();
		}
	}

	protected abstract void Initialize ();
	protected abstract void Loop ();
}
