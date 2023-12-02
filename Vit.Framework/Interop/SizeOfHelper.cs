using System.Runtime.InteropServices;

namespace Vit.Framework.Interop;

public static class SizeOfHelper<T> where T : unmanaged {
	public static readonly uint Size = (uint)Marshal.SizeOf( default(T) );
	public static readonly int SignedSize = Marshal.SizeOf( default(T) );
}
