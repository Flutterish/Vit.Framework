using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vit.Framework.Interop;

public static class InteropExtensions {
	public static PinnedHandle Pin ( this object? obj ) => new( obj );
	public static CString Cstr ( this string str ) => new( str );

	public static unsafe string GetString ( byte* cstr ) {
		return Marshal.PtrToStringUTF8( (nint)cstr )!;
	}

	public unsafe static T* Data<T> ( this T[] array ) where T : unmanaged {
		return (T*)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( array ) );
	}

	public unsafe static T** Data<T> ( this T*[] array ) where T : unmanaged {
		return (T**)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( array ) );
	}

	public unsafe static T* Data<T> ( this List<T> list ) where T : unmanaged {
		return (T*)Unsafe.AsPointer( ref MemoryMarshal.AsRef<T>( MemoryMarshal.Cast<T, byte>( CollectionsMarshal.AsSpan( list ) ) ) );
	}
}
