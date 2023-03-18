namespace Vit.Framework.Threading;

public abstract class AppThread : IDisposable, IAsyncDisposable {
	public readonly string Name;
	Thread? nativeThread;

	public AppThread ( string name ) {
		Name = name;
	}

	object runLock = new();
	public ThreadState State { get; private set; }
	public void Start () {
		if ( IsDisposed )
			return;

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
		if ( DateTime.Now < sleepsUntil )
			Sleep( sleepsUntil - DateTime.Now );

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

	public bool RunOnce () {
		if ( IsDisposed )
			return false;

		lock ( runLock ) {
			if ( State != ThreadState.Stopped ) {
				throw new InvalidOperationException( "Cannot single-thread an already running app thread" );
			}

			State = ThreadState.Running;
		}

		if ( DateTime.Now < sleepsUntil ) {
			State = ThreadState.Stopped;
			haltTaskSource?.TrySetResult();
			return false;
		}

		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		Loop();
		State = ThreadState.Stopped;
		haltTaskSource?.TrySetResult();
		return true;
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

	protected void Sleep ( int millisecondsTimeout ) {
		if ( Thread.CurrentThread == nativeThread )
			Thread.Sleep( millisecondsTimeout );
		else
			sleepsUntil = DateTime.Now + TimeSpan.FromMilliseconds( millisecondsTimeout );
	}
	protected void Sleep ( TimeSpan timeout ) {
		if ( Thread.CurrentThread == nativeThread )
			Thread.Sleep( timeout );
		else
			sleepsUntil = DateTime.Now + timeout;
	}

	DateTime sleepsUntil = DateTime.MinValue;

	public bool IsDisposed { get; private set; }
	protected virtual void Dispose ( bool disposing ) { }

	~AppThread () {
	    Dispose(disposing: false);
		IsDisposed = true;
		StopAsync();
	}

	public void Dispose () {
		if ( IsDisposed )
			return;

		GC.SuppressFinalize( this );
		Dispose( disposing: true );
		IsDisposed = true;
		StopAsync();
	}

	public ValueTask DisposeAsync () {
		if ( IsDisposed )
			return new ValueTask(  );

		GC.SuppressFinalize( this );
		Dispose( disposing: true );
		IsDisposed = true;
		return new ValueTask( StopAsync() );
	}
}

public enum ThreadState : byte {
	Stopped,
	Running,
	Halting
}