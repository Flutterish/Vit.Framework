using System.Text;
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
		public ushort Length;
		public ushort Language;

		static Type? selectType ( ushort format ) {
			return format switch {
				0 => typeof( Subtable0 ),
				4 => typeof( Subtable4 ),
				6 => typeof( Subtable6 ),
				_ => null
			};
		}

		public abstract IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding, int rangeStart, int rangeEnd );
		public abstract IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding );
	}

	public class Subtable0 : Subtable {
		[Size( 256 )]
		public BinaryArrayView<byte> GlyphIdArray;

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding, int rangeStart, int rangeEnd ) {
			if ( encoding == EncodingType.Unicode ) {
				rangeEnd = int.Min( rangeEnd, 256 );
				for ( int i = rangeStart; i < rangeEnd; i++ ) {
					if ( GlyphIdArray[i] != 0 )
						yield return (new Rune(i), new GlyphId( GlyphIdArray[i] ));
				}

				yield break;
			}

			for ( int i = 0; i < 256; i++ ) {
				if ( GlyphIdArray[i] == 0 )
					continue;

				var rune = encoding.Decode( i );
				if ( rune.Value >= rangeStart && rune.Value < rangeEnd )
					yield return (rune, new GlyphId( GlyphIdArray[i] ));
			}
		}

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding ) {
			throw new NotImplementedException();
		}
	}

	public class Subtable4 : Subtable {
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

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding, int rangeStart, int rangeEnd ) {
			if ( encoding == EncodingType.Unicode ) {
				// TODO implement optimized

				//yield break;
			}

			for ( var i = 0; i < StartCodes.Length; i++ ) {
				var start = StartCodes[i];
				var end = EndCodes[i];
				var idDelta = IdDeltas[i];
				var idRangeOffset = IdRangeOffsets[i];

				for ( ushort c = start; c < end; c++ ) {
					var rune = encoding.Decode( c );
					if ( rune.Value < rangeStart || rune.Value >= rangeEnd )
						continue;

					ushort id;
					if ( idRangeOffset == 0 )
						id = (ushort)( idDelta + c );
					else {
						id = GlyphIdArray[i - StartCodes.Length + idRangeOffset / 2 + c - start];
					}

					yield return (rune, new GlyphId(id));
				}
			}
		}

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding ) {
			for ( var i = 0; i < StartCodes.Length; i++ ) {
				var start = StartCodes[i];
				var end = EndCodes[i];
				var idDelta = IdDeltas[i];
				var idRangeOffset = IdRangeOffsets[i];

				for ( ushort c = start; c < end; c++ ) {
					var rune = encoding.Decode( c );
					ushort id;
					if ( idRangeOffset == 0 )
						id = (ushort)(idDelta + c);
					else {
						id = GlyphIdArray[i - StartCodes.Length + idRangeOffset / 2 + c - start];
					}

					yield return (rune, new GlyphId( id ));
				}
			}
		}
	}

	public class Subtable6 : Subtable {
		public ushort FirstCode;
		public ushort EntryCount;
		[Size( nameof( EntryCount ) )]
		public BinaryArrayView<ushort> GlyphIdArray;

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding, int rangeStart, int rangeEnd ) {
			rangeStart -= FirstCode;
			rangeEnd -= FirstCode;

			if ( encoding == EncodingType.Unicode ) {
				rangeEnd = int.Min( rangeEnd, EntryCount );
				rangeStart = int.Max( rangeStart, 0 );
				for ( int i = rangeStart; i < rangeEnd; i++ ) {
					if ( GlyphIdArray[i] != 0 )
						yield return (new Rune(i + FirstCode), new GlyphId( GlyphIdArray[i] ));
				}

				yield break;
			}

			for ( int i = 0; i < EntryCount; i++ ) {
				if ( GlyphIdArray[i] == 0 )
					continue;

				var rune = encoding.Decode( i + FirstCode );
				if ( rune.Value >= rangeStart && rune.Value < rangeEnd )
					yield return (rune, new GlyphId( GlyphIdArray[i] ));
			}
		}

		public override IEnumerable<(Rune rune, GlyphId id)> Enumerate ( EncodingType encoding ) {
			for ( int i = 0; i < EntryCount; i++ ) {
				if ( GlyphIdArray[i] == 0 )
					continue;

				var rune = encoding.Decode( i + FirstCode );
				yield return (rune, new GlyphId( GlyphIdArray[i] ));
			}
		}
	}
}
