using Vit.Framework.Memory;

namespace Vit.Framework.Threading.Synchronisation;

public class TripleBuffer {
	object useLock = new();

	int? writeIndex;
	int? readIndex;
	int availableIndex;
	bool isNew;
	public DisposeAction<(TripleBuffer buffer, int index)> GetForWrite ( out int index ) {
		lock ( useLock ) {
			if ( writeIndex.HasValue )
				throw new InvalidOperationException( "Triple buffer is already being written to" );

			index = availableIndex == 0 ? 1 : 0;
			if ( readIndex is int read && index == read ) {
				index = availableIndex == 2 ? 1 : 2;
			}

			writeIndex = index;
		}

		return new( (this, index), static v => {
			lock ( v.buffer.useLock ) {
				v.buffer.writeIndex = null;
				v.buffer.availableIndex = v.index;
				v.buffer.isNew = true;
			}
		} );
	}

	public DisposeAction<(TripleBuffer buffer, int index)> GetForRead ( out int index, out bool isNew ) {
		lock ( useLock ) {
			if ( readIndex.HasValue )
				throw new InvalidOperationException( "Triple buffer is already being read from" );

			isNew = this.isNew;
			this.isNew = false;
			readIndex = index = availableIndex;
		}

		return new( (this, index), static v => {
			lock ( v.buffer.useLock ) {
				v.buffer.readIndex = null;
			}
		} );
	}

	public bool TryGetForRead ( out int index, out DisposeAction<(TripleBuffer buffer, int index)> disposeAction ) {
		lock ( useLock ) {
			if ( readIndex.HasValue )
				throw new InvalidOperationException( "Triple buffer is already being read from" );

			if ( !isNew ) {
				index = -1;
				disposeAction = default;
				return false;
			}

			this.isNew = false;
			readIndex = index = availableIndex;
		}

		disposeAction = new( (this, index), static v => {
			lock ( v.buffer.useLock ) {
				v.buffer.readIndex = null;
			}
		} );
		return true;
	}
}
