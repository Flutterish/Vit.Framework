﻿using System.Text;

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

	public UnicodeExtendedGraphemeCluster ( byte* bytes, int length ) {
		ptr = bytes;
		this.length = length;
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

	public CodepointEnumerator GetEnumerator () => new( this );

	public struct CodepointEnumerator {
		UnicodeExtendedGraphemeCluster source;
		int index;

		public CodepointEnumerator ( UnicodeExtendedGraphemeCluster source ) {
			this.source = source;
			this.index = -1;
		}

		public bool MoveNext () {
			index++;
			return index < source.CodepointLength;
		}

		public uint Current => source.GetCodepoint( index );
	}
}
