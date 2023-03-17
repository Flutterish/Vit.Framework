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

	public void Run ( App app ) {
		app.Initialize( this );
		
		while ( true ) {
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
	}

	public abstract Window CreateWindow ( RenderingApi renderingApi );

	public abstract IEnumerable<RenderingApi> SupportedRenderingApis { get; }

	public void Quit () {
		Dispose();
	}
	public abstract void Dispose ();
}
