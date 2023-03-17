using System.Buffers;
using System.Collections.Concurrent;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Threading;
using Vit.Framework.Windowing;

namespace Vit.Framework.Platform;

public abstract class Host : IDisposable {
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
	public void RegisterThread ( AppThread thread ) {
		appThreads.Add( thread );
	}

	App? primaryApp;
	public App? PrimaryApp => primaryApp;
	public void Run ( App app ) {
		if ( Interlocked.CompareExchange( ref primaryApp, app, null ) != null )
			throw new InvalidOperationException( "Host can only run 1 primary app at a time" );

		app.Initialize( this );
		
		while ( !HasQuit ) {
			var threadCount = appThreads.Count;
			var threads = ArrayPool<AppThread>.Shared.Rent( threadCount );
			appThreads.CopyTo( threads, 0 );
			var mode = threadingMode;
			foreach ( var i in threads.AsSpan( 0, threadCount ) ) {
				if ( mode == ThreadingMode.SingleThreaded ) {
					if ( i.State == Threading.ThreadState.Stopped )
						i.RunOnce();
					else
						i.StopAsync();
				}
				else {
					if ( i.State == Threading.ThreadState.Stopped )
						i.Start();
				}
			}

			ArrayPool<AppThread>.Shared.Return( threads );

			if ( mode == ThreadingMode.Multithreaded )
				Thread.Sleep( 10 );
		}

		foreach ( var i in appThreads.ToArray() )
			i.StopAsync();
	}

	public abstract Window CreateWindow ( RenderingApi renderingApi );

	public abstract IEnumerable<RenderingApi> SupportedRenderingApis { get; }

	public bool HasQuit { get; private set; }
	public void Quit () {
		Dispose();
	}
	public void Dispose () {
		if ( HasQuit )
			return;

		GC.SuppressFinalize( this );
		Dispose( true );
		HasQuit = true;
	}
	public abstract void Dispose ( bool isDisposing );

	~Host () {
		Dispose( false );
		HasQuit = true;
	}
}
