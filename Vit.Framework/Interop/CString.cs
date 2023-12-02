using System.Runtime.InteropServices;
using System.Text;

namespace Vit.Framework.Interop;

/// <summary>
/// UTF-8 encoded null-terminated string pointer.
/// </summary>
public unsafe struct CString {
	byte* ptr;

	public CString ( byte* ptr ) {
		this.ptr = ptr;
	}

	public CString ( nint ptr ) {
		this.ptr = (byte*)ptr;
	}

	public static CString CreateStaticPinned ( string str ) {
		return CreatePinned( str ).str;
	}

	public static (CString str, GCHandle handle) CreatePinned ( string str ) {
		var data = new byte[Encoding.UTF8.GetByteCount( str ) + 1];
		Encoding.UTF8.GetBytes( str.AsSpan(), data.AsSpan( 0, data.Length - 1 ) );
		data[^1] = 0;
		var pin = GCHandle.Alloc( data, GCHandleType.Pinned );

		return (new CString( pin.AddrOfPinnedObject() ), pin);
	}

	/// <summary>
	/// Returns the text this cstring represents.
	/// </summary>
	public override string ToString () {
		return (string)this;
	}

	public static implicit operator string ( CString cstr )
		=> Marshal.PtrToStringUTF8( (nint)(byte*)cstr )!;

	public static explicit operator CString ( byte* str )
		=> new( str );

	public static explicit operator CString ( nint str )
		=> new( str );

	public static unsafe implicit operator byte* ( CString str ) {
		return str.ptr;
	}
}

public static unsafe class CStringExtensions {
	public static byte*[] MakeArray ( this IReadOnlyList<CString> array ) {
		var data = new byte*[array.Count];
		for ( int i = 0; i < array.Count; i++ )
			data[i] = array[i];
		return data;
	}
}