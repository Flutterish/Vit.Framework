using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Vit.Framework.Interop;

public unsafe struct CString {
	byte* ptr;
	byte[]? data;

	public CString ( byte* ptr ) {
		this.ptr = ptr;
	}

	public CString ( nint ptr ) {
		this.ptr = (byte*)ptr;
	}

	public CString ( string str ) {
		data = new byte[Encoding.UTF8.GetByteCount( str ) + 1];
		Encoding.UTF8.GetBytes( str.AsSpan(), data.AsSpan( 0, data.Length - 1 ) );
		data[^1] = 0;

		ptr = (byte*)Unsafe.AsPointer( ref MemoryMarshal.GetArrayDataReference( data ) );
	}

	public override string ToString () {
		return (string)this;
	}

	public static implicit operator string ( CString cstr )
		=> Marshal.PtrToStringUTF8( (nint)cstr.ptr )!;

	public static implicit operator CString ( string str )
		=> new( str );

	public static explicit operator CString ( byte* str )
		=> new( str );

	public static explicit operator CString ( nint str )
		=> new( str );

	public static unsafe implicit operator byte* ( CString str )
		=> str.ptr;
}

public static unsafe class CStringExtensions {
	public static byte*[] MakeArray ( this IReadOnlyList<CString> array ) {
		var data = new byte*[array.Count];
		for ( int i = 0; i < array.Count; i++ )
			data[i] = array[i];
		return data;
	}
}