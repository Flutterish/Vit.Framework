namespace Vit.Framework.Threading;

public class AppThread {
	readonly Thread nativeThread;

	public AppThread ( string name ) {
		nativeThread = new Thread( threadLoop ) { Name = name };
	}

	void threadLoop () {

	}
}
