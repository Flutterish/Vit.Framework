using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Vit.Framework.Interop;

public static class InteropExtensions {
	public static PinnedHandle Pin ( this object? obj ) => new( obj );

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

	public unsafe static TTo BitCast<TFrom, TTo> ( this TFrom from ) where TTo : unmanaged where TFrom : unmanaged {
		return Unsafe.As<TFrom, TTo>( ref from );
	}

	public unsafe static Span<byte> ToBytes<T> ( ref this T self ) where T : unmanaged {
		return MemoryMarshal.AsBytes( MemoryMarshal.CreateSpan( ref self, 1 ) );
	}
}
