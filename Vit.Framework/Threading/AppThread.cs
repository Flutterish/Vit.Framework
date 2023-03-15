namespace Vit.Framework.Threading;

public class AppThread {
	readonly Thread nativeThread;

	public AppThread () {
		nativeThread = new Thread( threadLoop );
	}

	void threadLoop () {

	}
}
