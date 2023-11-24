using Vit.Framework.Threading;

namespace Vit.Framework;

public abstract class App {
	public readonly string Name;

	public readonly AppThreadRunner ThreadRunner;
	public App ( string name ) {
		Name = name;
		ThreadRunner = new( $"<{Name}> Thread Runner" );
	}

	public void Run () {
		Initialize();
		while ( !HasQuit ) {
			if ( !ThreadRunner.RunOnce() )
				Thread.Sleep( 1 );
		}

		ThreadRunner.Dispose();
	}

	public bool HasQuit { get; private set; }
	public void Quit () {
		if ( HasQuit )
			return;

		HasQuit = true;
	}

	protected virtual void BeforeQuit () { }

	protected abstract void Initialize ();
}
