using System.Diagnostics;

namespace Vit.Framework.Memory;

public abstract class DisposableObject : IDisposable {
	public bool IsDisposed { get; private set; }
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected virtual string IncorrectDisposalMessage => $"{GetType().Name} was not disposed correctly (collected by GC)";
	protected abstract void Dispose ( bool disposing );
	
	~DisposableObject () {
		throwIfGarbageCollected();

		Dispose( disposing: false );
		IsDisposed = true;
	}

	public void Dispose () {
		if ( IsDisposed ) {
			throwIfDisposed();
			return;
		}

		Dispose( disposing: true );
		IsDisposed = true;
		GC.SuppressFinalize( this );
	}

	[Conditional( "DEBUG" )]
	void throwIfDisposed () {
		throw new InvalidOperationException( "Object was disposed more than once" );
	}

	[Conditional( "DEBUG" )]
	void throwIfGarbageCollected () {
		throw new InvalidOperationException( IncorrectDisposalMessage );
	}
}