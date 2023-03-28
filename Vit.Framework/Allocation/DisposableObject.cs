using System.Diagnostics;

namespace Vit.Framework.Allocation;

public abstract class DisposableObject : IDisposable {
	public bool IsDisposed { get; private set; }
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected virtual string IncorrectDisposalMessage => $"{GetType().Name} was not disposed correctly (collected by GC)";
	protected abstract void Dispose ( bool disposing );

	~DisposableObject () {
		if ( Debugger.IsAttached )
			throw new InvalidOperationException( IncorrectDisposalMessage );

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
}