using System.Text;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public ref struct ByteString {
	public required ReadOnlySpan<byte> Bytes;
	public int Length => Bytes.Length;

	public char this[int index] => (char)Bytes[index];

	public ByteString Slice ( int start ) {
		return new() { Bytes = Bytes.Slice( start ) };
	}

	public ByteString Slice ( int start, int length ) {
		return new() { Bytes = Bytes.Slice( start, length ) };
	}

	public static bool operator == ( ByteString left, ByteString right )
		=> left.Bytes.SequenceEqual( right.Bytes );
	public static bool operator != ( ByteString left, ByteString right )
		=> !left.Bytes.SequenceEqual( right.Bytes );

	public override string ToString () {
		return Encoding.UTF8.GetString( Bytes );
	}
}

public struct HeapByteString {
	byte[] data;
	public HeapByteString ( string data ) {
		this.data = Encoding.UTF8.GetBytes( data );
	}

	public static implicit operator HeapByteString ( string data )
		=> new( data );

	public static implicit operator ByteString ( HeapByteString str )
		=> new() { Bytes = str.data };
}