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

	public AppThreadRunner ( string name ) : base( name ) { }

	public void RegisterThread ( AppThread thread ) {
		appThreads.Add( thread );
	}

	protected override void Initialize () { }

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

		ArrayPool<AppThread>.Shared.Return( threads );

		if ( mode == ThreadingMode.Multithreaded )
			Sleep( 10 );
	}

	protected override void Dispose ( bool disposing ) {
		StopAsync().ContinueWith( _ => {
			foreach ( var i in appThreads.ToArray() )
				i.StopAsync();
		} );
	}
}
