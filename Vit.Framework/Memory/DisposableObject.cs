using System.Diagnostics;
using Vit.Framework.Exceptions;

namespace Vit.Framework.Memory;

public abstract class DisposableObject : IDisposable {
	public bool IsDisposed { get; private set; }
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	protected virtual string IncorrectDisposalMessage => $"{GetType().Name} was not disposed correctly (collected by GC)";
	protected abstract void Dispose ( bool disposing );

	public DisposableObject () {
		addToAliveCache();
	}

	static Dictionary<Type, ulong> aliveCache = new();

	[Conditional( "DEBUG" )]
	void addToAliveCache () {
		var type = GetType();
		lock ( aliveCache ) {
			if ( aliveCache.TryGetValue( type, out ulong value ) )
				aliveCache[type] = value + 1;
			else
				aliveCache[type] = 1;
		}
	}

	[Conditional( "DEBUG" )]
	void removeFromAliveCache () {
		lock ( aliveCache ) {
			aliveCache![GetType()]--;
		}
	}

	[Conditional("DEBUG")]
	public static void ValidateEverythingIsDisposed () {
		lock ( aliveCache ) {
			var stillAlive = aliveCache.Where( x => x.Value != 0 );
			if ( stillAlive.Any() ) {
				throw new InvalidStateException( $"Expected all {nameof(DisposableObject)}s to be disposed at this point, but some were still alive: \n\tCount\tType\n\t-----\t----\n\t{string.Join("\n\t", stillAlive.Select(
					x => $"{x.Value}\t{x.Key.FullName}"
				))}" );
			}
		}
	}

	~DisposableObject () {
		throwIfGarbageCollected();

		Dispose( disposing: false );
		IsDisposed = true;
	}

	public void Dispose () {
		if ( IsDisposed )
			return;

		removeFromAliveCache();
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