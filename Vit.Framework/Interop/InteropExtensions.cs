using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Vit.Framework.Memory;

namespace Vit.Framework.Interop;

public static class InteropExtensions {
	public static PinnedHandle Pin ( this object? obj ) => new( obj );
	public static CString Cstr ( this string str ) => new( str );

	public static unsafe string GetString ( byte* cstr ) {
		return Marshal.PtrToStringUTF8( (nint)cstr )!;
	}

	public static unsafe string GetString ( byte* cstr, int length ) {
		var data = new Span<byte>( cstr, length );
		var index = data.IndexOf( (byte)0 );
		if ( index != -1 )
			data = data[..index];

		return Encoding.UTF8.GetString( data );
	}

	public static Span<T> AsSpan<T> ( this List<T> list )
		=> CollectionsMarshal.AsSpan( list );

	public unsafe static T* Data<T> ( this T[] array ) where T : unmanaged {
		return (T*)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( array ) );
	}

	public unsafe static T* Data<T> ( this RentedArray<T> array ) where T : unmanaged {
		return array.AsSpan().Data();
	}

	public unsafe static nint DataPtr<T> ( this T[] array ) where T : unmanaged {
		return (nint)(T*)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( array ) );
	}

	public unsafe static T* Data<T> ( this ImmutableArray<T> array ) where T : unmanaged {
		return array.AsSpan().Data();
	}

	public unsafe static T* Data<T> ( this Span<T> span ) where T : unmanaged {
		return (T*)Unsafe.AsPointer( ref MemoryMarshal.GetReference( span ) );
	}

	public unsafe static T* Data<T> ( this ReadOnlySpan<T> span ) where T : unmanaged {
		return (T*)Unsafe.AsPointer( ref MemoryMarshal.GetReference( span ) );
	}

	public unsafe static T** Data<T> ( this T*[] array ) where T : unmanaged {
		return (T**)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( array ) );
	}

	public unsafe static T* Data<T> ( this List<T> list ) where T : unmanaged {
		return CollectionsMarshal.AsSpan( list ).Data();
	}

	public unsafe static TTo BitCast<TFrom, TTo> ( this TFrom from ) where TTo : unmanaged where TFrom : unmanaged {
		return MemoryMarshal.Cast<TFrom, TTo>( MemoryMarshal.CreateSpan( ref from, 1 ) )[0];
	}
}
