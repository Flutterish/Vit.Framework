using System.Buffers;
using System.Collections.Concurrent;

namespace Vit.Framework.Threading;

public class AppThreadRunner : AppThread {
	ThreadingMode threadingMode = ThreadingMode.Multithreaded;
	public ThreadingMode ThreadingMode {
		get => threadingMode;
		set {
			if ( threadingMode == value )
				return;

			threadingMode = value;
		}
	}

	ConcurrentBag<AppThread> appThreads = new();

	public AppThreadRunner ( string name ) : base( name ) {
		RateLimit = double.PositiveInfinity;
	}

	public void RegisterThread ( AppThread thread ) {
		appThreads.Add( thread );
	}

	protected override bool Initialize () { return true; }

	protected override void Loop () {
		var threadCount = appThreads.Count;
		var threads = ArrayPool<AppThread>.Shared.Rent( threadCount );
		appThreads.CopyTo( threads, 0 );
		var mode = threadingMode;
		foreach ( var i in threads.AsSpan( 0, threadCount ) ) {
			if ( mode == ThreadingMode.SingleThreaded ) {
				if ( i.State == ThreadState.Stopped )
					i.RunOnce();
				else
					i.StopAsync();
			}
			else {
				if ( i.State == ThreadState.Stopped )
					i.Start();
			}
		}

		Array.Fill( threads, null );
		ArrayPool<AppThread>.Shared.Return( threads );

		if ( mode == ThreadingMode.Multithreaded )
			Sleep( 10 );
	}

	protected override void Dispose ( bool disposing ) {
		StopAsync().ContinueWith( _ => {
			var threads = appThreads.ToArray();
			Task[] tasks = new Task[threads.Length];
			for ( int i = 0; i < threads.Length; i++ ) {
				tasks[i] = threads[i].DisposeAsync().AsTask();
			}

			appThreads.Clear();
			foreach ( var i in tasks )
				i.Wait();
		} ).Wait();
	}
}
