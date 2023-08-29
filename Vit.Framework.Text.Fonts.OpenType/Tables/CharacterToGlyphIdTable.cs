using System.Diagnostics;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class CharacterToGlyphIdTable : Table {
	public ushort Version;
	public ushort TableCount;

	[Size( nameof( TableCount ) )]
	public BinaryArrayView<EncodingRecord> EncodingRecords;

	public struct EncodingRecord {
		public ushort PlatformID;
		public ushort EncodingID;
		public Offset32 SubtableOffset;

		[DataOffset( nameof( SubtableOffset ) )]
		public BinaryView<Subtable> Subtable;
	}

	[TypeSelector( nameof( selectType ) )]
	public abstract class Subtable {
		public ushort Format;

		static Type? selectType ( ushort format ) {
			return format switch {
				0 => typeof( Subtable0 ),
				4 => typeof( Subtable4 ),
				6 => typeof( Subtable6 ),
				12  => typeof( Subtable12 ),
				_ => throw new NotImplementedException()
			};
		}

		public abstract class GlyphEnumerator {
			public abstract bool MoveNext ();
			public abstract GlyphId GlyphId { get; }
			public abstract ReadOnlySpan<byte> GlyphVector { get; }
		}

		/// <summary>
		/// Enumerates all glyphs in this table which match the key and may differ in the first byte of the last codepoint (last byte in big-endian).
		/// </summary>
		public abstract IEnumerable<(byte lastByte, GlyphId id)> EnumeratePage ( UnicodeExtendedGraphemeCluster cluster );
		/// <summary>
		/// Enumerates all glyphs in this table.
		/// </summary>
		public abstract GlyphEnumerator EnumerateAll ();
	}

	public class Subtable0 : Subtable {
		public ushort Length;
		public ushort Language;

		[Size( 256 )]
		public BinaryArrayView<byte> GlyphIdArray;

		public override IEnumerable<(byte lastByte, GlyphId id)> EnumeratePage ( UnicodeExtendedGraphemeCluster cluster ) {
			throw new NotImplementedException();
			//if ( encoding == EncodingType.Unicode ) {
			//	rangeEnd = int.Min( rangeEnd, 256 );
			//	for ( int i = rangeStart; i < rangeEnd; i++ ) {
			//		if ( GlyphIdArray[i] != 0 )
			//			yield return (new Rune(i), new GlyphId( GlyphIdArray[i] ));
			//	}

			//	yield break;
			//}

			//for ( int i = 0; i < 256; i++ ) {
			//	if ( GlyphIdArray[i] == 0 )
			//		continue;

			//	var rune = encoding.Decode( i );
			//	if ( rune.Value >= rangeStart && rune.Value < rangeEnd )
			//		yield return (rune, new GlyphId( GlyphIdArray[i] ));
			//}
		}

		public override GlyphEnumerator EnumerateAll () {
			throw new NotImplementedException();
		}
	}

	public class Subtable4 : Subtable {
		public ushort Length;
		public ushort Language;

		public ushort SegCountX2;
		public ushort SearchRange;
		public ushort EntrySelector;
		public ushort RangeShift;
		[Size( nameof( SegCountX2 ), 0.5 )]
		public BinaryArrayView<ushort> EndCodes;
		public ushort ReservePad;
		[Size( nameof( SegCountX2 ), 0.5 )]
		public BinaryArrayView<ushort> StartCodes;
		[Size( nameof( SegCountX2 ), 0.5 )]
		public BinaryArrayView<ushort> IdDeltas;
		[Size( nameof( SegCountX2 ), 0.5 )]
		public BinaryArrayView<ushort> IdRangeOffsets;

		[Size( nameof( getSize ) )]
		public BinaryArrayView<ushort> GlyphIdArray;

		static int getSize ( BinaryArrayView<ushort> startCodes, BinaryArrayView<ushort> endCodes ) {
			var total = 0;
			for ( int i = 0; i < startCodes.Length; i++ ) {
				total += endCodes[i] - startCodes[i];
			}
			return total;
		}

		public override IEnumerable<(byte lastByte, GlyphId id)> EnumeratePage ( UnicodeExtendedGraphemeCluster cluster ) {
			if ( cluster.CodepointLength != 1 )
				yield break;

			uint codepoint = cluster[0];
			if ( codepoint > 0xffff )
				yield break;

			var pageStart = codepoint / 256 * 256;
			var pageEnd = pageStart + 256;

			for ( int i = 0; i < StartCodes.Length; i++ ) { // TODO binary search the first segment
				var end = EndCodes[i];
				if ( end < pageStart )
					continue;

				var start = StartCodes[i];
				if ( start >= pageEnd )
					yield break;

				var idDelta = IdDeltas[i];
				var idRangeOffset = IdRangeOffsets[i];

				for ( int c = start; c < end; c++ ) {
					if ( c < pageStart || c >= pageEnd )
						continue;

					ushort id;
					if ( idRangeOffset == 0 ) {
						id = (ushort)(idDelta + c);
					}
					else {
						id = GlyphIdArray[i - StartCodes.Length + idRangeOffset / 2 + c - start];
					}

					Debug.Assert( (c & 0xff00) == (codepoint & 0xff00) );
					yield return ((byte)(c & 0x00ff), new GlyphId( id ));
				}
			}
		}

		public override GlyphEnumerator EnumerateAll () {
			throw new NotImplementedException();
			//for ( var i = 0; i < StartCodes.Length; i++ ) {
			//	var start = StartCodes[i];
			//	var end = EndCodes[i];
			//	var idDelta = IdDeltas[i];
			//	var idRangeOffset = IdRangeOffsets[i];

			//	for ( ushort c = start; c < end; c++ ) {
			//		var rune = encoding.Decode( c );
			//		ushort id;
			//		if ( idRangeOffset == 0 )
			//			id = (ushort)(idDelta + c);
			//		else {
			//			id = GlyphIdArray[i - StartCodes.Length + idRangeOffset / 2 + c - start];
			//		}

			//		yield return (rune, new GlyphId( id ));
			//	}
			//}
		}
	}

	public class Subtable6 : Subtable {
		public ushort Length;
		public ushort Language;

		public ushort FirstCode;
		public ushort EntryCount;
		[Size( nameof( EntryCount ) )]
		public BinaryArrayView<ushort> GlyphIdArray;

		public override IEnumerable<(byte lastByte, GlyphId id)> EnumeratePage ( UnicodeExtendedGraphemeCluster cluster ) {
			throw new NotImplementedException();
			//rangeStart -= FirstCode;
			//rangeEnd -= FirstCode;

			//if ( encoding == EncodingType.Unicode ) {
			//	rangeEnd = int.Min( rangeEnd, EntryCount );
			//	rangeStart = int.Max( rangeStart, 0 );
			//	for ( int i = rangeStart; i < rangeEnd; i++ ) {
			//		if ( GlyphIdArray[i] != 0 )
			//			yield return (new Rune(i + FirstCode), new GlyphId( GlyphIdArray[i] ));
			//	}

			//	yield break;
			//}

			//for ( int i = 0; i < EntryCount; i++ ) {
			//	if ( GlyphIdArray[i] == 0 )
			//		continue;

			//	var rune = encoding.Decode( i + FirstCode );
			//	if ( rune.Value >= rangeStart && rune.Value < rangeEnd )
			//		yield return (rune, new GlyphId( GlyphIdArray[i] ));
			//}
		}

		public override GlyphEnumerator EnumerateAll () {
			throw new NotImplementedException();
			//for ( int i = 0; i < EntryCount; i++ ) {
			//	if ( GlyphIdArray[i] == 0 )
			//		continue;

			//	var rune = encoding.Decode( i + FirstCode );
			//	yield return (rune, new GlyphId( GlyphIdArray[i] ));
			//}
		}
	}

	public class Subtable12 : Subtable {
		public ushort Reserved;
		public uint Length;
		public uint Language;
		public uint NumGroups;
		[Size(nameof(NumGroups))]
		public BinaryArrayView<SequentialMapGroup> Groups;

		public override IEnumerable<(byte lastByte, GlyphId id)> EnumeratePage ( UnicodeExtendedGraphemeCluster cluster ) {
			if ( cluster.CodepointLength != 1 )
				yield break;

			uint codepoint = cluster[0];
			var pageStart = codepoint / 256 * 256;
			var pageEnd = pageStart + 256;

			for ( int i = 0; i < NumGroups; i++ ) {
				var group = Groups[i];
				if ( group.EndCharCode < pageStart )
					continue;

				if ( group.StartCharCode >= pageEnd ) // TODO fuck knows if the end code is inclusive or not
					yield break;

				var limit = uint.Min( pageEnd, group.EndCharCode );
				for ( uint c = uint.Max( group.StartCharCode, pageStart ); c < limit; c++ ) {
					yield return ((byte)(c & 0x000000ff), new( group.StartGlyphId + (c - group.StartCharCode) ));
				}
			}
		}

		public override GlyphEnumerator EnumerateAll () {
			throw new NotImplementedException();
		}

		public struct SequentialMapGroup {
			public uint StartCharCode;
			public uint EndCharCode;
			public uint StartGlyphId;
		}
	}
}
