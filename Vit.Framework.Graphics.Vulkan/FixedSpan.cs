using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Graphics.Vulkan;

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
