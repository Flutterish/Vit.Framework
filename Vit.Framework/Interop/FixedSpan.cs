using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Interop;

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan1<T> where T : unmanaged {
	public T Item1;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan2<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan3<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public T Item3;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
}

[StructLayout( LayoutKind.Sequential )]
public struct FixedSpan4<T> where T : unmanaged {
	public T Item1;
	public T Item2;
	public T Item3;
	public T Item4;
	public unsafe T* Data () => (T*)Unsafe.AsPointer( ref Item1 );
}

public static class FixedSpan<T> where T : unmanaged {
	public static FixedSpan1<T> From ( T item1 ) => new() { Item1 = item1 };
	public static FixedSpan2<T> From ( T item1, T item2 ) => new() { Item1 = item1, Item2 = item2 };
	public static FixedSpan3<T> From ( T item1, T item2, T item3 ) => new() { Item1 = item1, Item2 = item2, Item3 = item3 };
	public static FixedSpan4<T> From ( T item1, T item2, T item3, T item4 ) => new() { Item1 = item1, Item2 = item2, Item3 = item3, Item4 = item4 };
}