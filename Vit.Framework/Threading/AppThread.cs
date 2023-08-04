namespace Vit.Framework.Threading;

public abstract class AppThread : IDisposable, IAsyncDisposable {
	public readonly string Name;
	Thread? nativeThread;

	public AppThread ( string name ) {
		Name = name;
	}

	object runLock = new();
	public ThreadState State { get; private set; }
	double rateLimit = 1000;
	public double RateLimit {
		get => Interlocked.CompareExchange( ref rateLimit, 0, 0 );
		set => Interlocked.Exchange( ref rateLimit, value );
	}
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
	public bool IsInitialized => isInitialized;
	void onThreadStart ( object? haltTaskSource ) {
		if ( DateTime.Now < sleepsUntil )
			Sleep( sleepsUntil - DateTime.Now );

		if ( !isInitialized ) {
			Initialize();
			isInitialized = true;
		}

		while ( State != ThreadState.Halting ) {
			runLoopWithRateLimit();
		}

		if ( IsBeingDisposed ) {
			Dispose( disposing: true );
			IsDisposed = true;
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

		runLoopWithRateLimit();

		if ( IsBeingDisposed ) {
			Dispose( disposing: true );
			IsDisposed = true;
		}
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
	void runLoopWithRateLimit () {
		var startTime = DateTime.Now;
		Loop();
		var deltaTime = DateTime.Now - startTime;
		var allotedTime = TimeSpan.FromSeconds( 1 ) / RateLimit;
		if ( allotedTime > deltaTime ) {
			Sleep( allotedTime - deltaTime );
		}
	}

	protected void Sleep ( int millisecondsTimeout ) {
		Sleep( TimeSpan.FromMilliseconds( millisecondsTimeout ) );
	}
	protected void Sleep ( TimeSpan timeout ) {
		if ( timeout >= sleepVariance ) {
			timeout -= sleepVariance;
			sleepVariance = TimeSpan.Zero;
		}
		else {
			sleepVariance -= timeout;
			timeout = TimeSpan.Zero;
		}

		sleepsUntil = DateTime.Now + timeout;
		if ( Thread.CurrentThread == nativeThread ) {
			Thread.Sleep( timeout );
			sleepVariance += DateTime.Now - sleepsUntil;
		}
	}

	DateTime sleepsUntil = DateTime.MinValue;
	TimeSpan sleepVariance; // positive = too much sleep

	public bool IsBeingDisposed { get; private set; }
	public bool IsDisposed { get; private set; }
	protected virtual void Dispose ( bool disposing ) { }

	public void Dispose () {
		if ( IsBeingDisposed )
			return;

		IsBeingDisposed = true;
		GC.SuppressFinalize( this );
		StopAsync().ContinueWith( _ => {
			if ( !IsDisposed ) {
				Dispose( disposing: true );
				IsDisposed = true;
			}
		} );
	}

	public ValueTask DisposeAsync () {
		if ( IsDisposed )
			return ValueTask.CompletedTask;

		if ( IsBeingDisposed )
			return new ValueTask( StopAsync() );

		IsBeingDisposed = true;
		GC.SuppressFinalize( this );
		return new ValueTask( StopAsync().ContinueWith( _ => {
			if ( !IsDisposed ) {
				Dispose( disposing: true );
				IsDisposed = true;
			}
		} ) );
	}

	~AppThread () {
		Dispose( disposing: false );
		IsDisposed = true;
		StopAsync();
	}
}

public enum ThreadState : byte {
	Stopped,
	Running,
	Halting
}