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
	bool isStopping;
	bool isRunning;
	void onThreadStart () {
		isRunning = true;
		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		while ( !isStopping ) {
			Loop();
		}
		isRunning = false;
		isStopping = false;
		stopTask?.SetResult();
	}

	TaskCompletionSource? stopTask;
	public Task Stop () {
		stopTask = new();
		isStopping = true;
		return stopTask.Task;
	}

	protected abstract void Initialize ();
	protected abstract void Loop ();
}
