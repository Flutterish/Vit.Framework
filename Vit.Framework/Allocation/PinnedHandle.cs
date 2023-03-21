using System.Runtime.InteropServices;

namespace Vit.Framework.Allocation;

public readonly ref struct PinnedHandle {
	readonly GCHandle handle;

	public PinnedHandle ( object? obj ) {
		handle = GCHandle.Alloc( obj, GCHandleType.Pinned );
	}

	public static implicit operator nint ( PinnedHandle handle )
		=> handle.handle.AddrOfPinnedObject();

	public void Dispose () {
		handle.Free();
	}
}
