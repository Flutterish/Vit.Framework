using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Interop;

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan1<T> where T : unmanaged {
	public T Item1;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Item1, 1 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan2<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Item1, 2 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan3<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public T Item3;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Item1, 3 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan4<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public T Item3;
	public T Item4;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
	public Span<T> AsSpan () => MemoryMarshal.CreateSpan( ref Item1, 4 );
}

public static class FixedSpan {
	public static FixedSpan1<T> From<T> ( T item1 ) where T : unmanaged 
		=> new() { Item1 = item1 };
	public static FixedSpan2<T> From<T> ( T item1, T item2 ) where T : unmanaged 
		=> new() { Item1 = item1, Item2 = item2 };
	public static FixedSpan3<T> From<T> ( T item1, T item2, T item3 ) where T : unmanaged 
		=> new() { Item1 = item1, Item2 = item2, Item3 = item3 };
	public static FixedSpan4<T> From<T> ( T item1, T item2, T item3, T item4 ) where T : unmanaged 
		=> new() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4 };
}