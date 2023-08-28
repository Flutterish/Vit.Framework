using System.Text;
using Vit.Framework.Interop;

namespace Vit.Framework.Text.Fonts;

/// <summary>
/// A span of unicode codepoints in little-endian order which represent a single extended grapheme cluster.
/// </summary>
/// <remarks>
/// This should be a ref-struct, but is not for the sake of being usable in iterators.
/// You should never store this type and only use it for processing.
/// </remarks>
public unsafe readonly struct UnicodeExtendedGraphemeCluster {
	readonly byte* ptr;
	readonly int length;

	public int ByteLength => length;
	public int CodepointLength => (length + 3) / 4;

	public UnicodeExtendedGraphemeCluster ( ReadOnlySpan<byte> bytes ) {
		ptr = bytes.Data();
		length = bytes.Length;
	}

	public ReadOnlySpan<byte> Bytes => new( ptr, length );
	public ReadOnlySpan<byte> GetCodepointBytes ( int index ) => Bytes.Slice( index * 4, 4 );
	public uint GetCodepoint ( int index ) {
		var bytes = GetCodepointBytes( index );

		return ((uint)bytes[3] << 24) | ((uint)bytes[2] << 16) | ((uint)bytes[1] << 8) | ((uint)bytes[0]);
	}

	public uint this[int index] => GetCodepoint( index );

	public override string ToString () {
		return Encoding.UTF32.GetString( Bytes );
	}
}
