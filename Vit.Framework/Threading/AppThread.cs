namespace Vit.Framework.Threading;

public abstract class AppThread {
	public readonly string Name;
	Thread? nativeThread;

	public AppThread ( string name ) {
		Name = name;
	}

	object runLock = new();
	public ThreadState State { get; private set; }
	public void Start () {
		lock ( runLock ) {
			if ( State != ThreadState.Stopped ) {
				throw new InvalidOperationException( "Cannot start an already running app thread" );
			}

			State = ThreadState.Running;

			nativeThread = new Thread( onThreadStart ) { Name = Name };
			nativeThread.Start( haltTaskSource = new() );
		}
	}

	bool isInitialized;
	void onThreadStart ( object? haltTaskSource ) {
		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		while ( State != ThreadState.Halting ) {
			Loop();
		}
		nativeThread = null;
		State = ThreadState.Stopped;
		(haltTaskSource as TaskCompletionSource)?.SetResult();
	}

	public void RunOnce () {
		lock ( runLock ) {
			if ( State != ThreadState.Stopped ) {
				throw new InvalidOperationException( "Cannot single-thread an already running app thread" );
			}

			State = ThreadState.Running;
		}

		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		Loop();
		State = ThreadState.Stopped;
		haltTaskSource?.TrySetResult();
	}

	TaskCompletionSource? haltTaskSource;
	public Task StopAsync () {
		lock ( runLock ) {
			if ( State != ThreadState.Running )
				return Task.CompletedTask;

			State = ThreadState.Halting;
			return haltTaskSource?.Task ?? Task.CompletedTask;
		}
	}

	public void Stop () {
		StopAsync().Wait();
	}

	protected abstract void Initialize ();
	protected abstract void Loop ();
}

public enum ThreadState : byte {
	Stopped,
	Running,
	Halting
}