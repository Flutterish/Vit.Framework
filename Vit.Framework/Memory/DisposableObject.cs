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
		if ( IsDisposed )
			return;

		Dispose( disposing: true );
		IsDisposed = true;
		GC.SuppressFinalize( this );
	}

	[Conditional( "DEBUG" )]
	void throwIfGarbageCollected () {
		throw new InvalidOperationException( IncorrectDisposalMessage );
	}

	[Conditional( "DEBUG" )]
	protected void ThrowIfDisposed () {
		if ( IsDisposed )
			throw new InvalidOperationException( "Object used after being disposed" );
	}
}