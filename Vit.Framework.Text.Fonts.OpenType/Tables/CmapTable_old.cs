using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class CmapTable_old : Table {
	public ushort Version;
	public ushort TableCount;

	[Size(nameof(TableCount))]
	public EncodingRecord[] EncodingRecords = null!;

	public struct EncodingRecord {
		public ushort PlatformID;
		public ushort EncodingID;
		public Offset32 SubtableOffset;

		[DataOffset(nameof(SubtableOffset))]
		public Subtable Subtable;
	}

	[TypeSelector(nameof(selectType))]
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

		public abstract IEnumerable<(ushort charcode, GlyphId id)> Glyphs { get; }
	}

	public class Subtable0 : Subtable {
		[Size(256)]
		public byte[] GlyphIdArray = null!;

		public override IEnumerable<(ushort, GlyphId)> Glyphs {
			get {
				for ( ushort i = 0; i < 256; i++ ) {
					if ( GlyphIdArray[i] != 0 )
						yield return (i, new GlyphId( GlyphIdArray[i] ));
				}
			}
		}
	}

	public class Subtable4 : Subtable {
		public ushort SegCountX2;
		public ushort SearchRange;
		public ushort EntrySelector;
		public ushort RangeShift;
		[Size(nameof(SegCountX2), 0.5)]
		public ushort[] EndCodes = null!;
		public ushort ReservePad;
		[Size(nameof(SegCountX2), 0.5)]
		public ushort[] StartCodes = null!;
		[Size(nameof(SegCountX2), 0.5)]
		public ushort[] IdDeltas = null!;
		[Size(nameof(SegCountX2), 0.5)]
		public ushort[] IdRangeOffsets = null!;

		[Size(nameof(getSize))]
		public ushort[] GlyphIdArray = null!;

		static int getSize ( ushort[] startCodes, ushort[] endCodes ) {
			var total = 0;
			for ( int i = 0; i < startCodes.Length; i++ ) {
				total += endCodes[i] - startCodes[i];
			}
			return total;
		}

		public override IEnumerable<(ushort, GlyphId)> Glyphs {
			get {
				for ( var i = 0; i < StartCodes.Length; i++ ) {
					var start = StartCodes[i];
					var end = EndCodes[i];
					var idDelta = IdDeltas[i];
					var idRangeOffset = IdRangeOffsets[i];

					for ( ushort c = start; c < end; c++ ) {
						ushort id;
						if ( idRangeOffset == 0 )
							id = (ushort)( idDelta + c );
						else {
							id = GlyphIdArray[i - StartCodes.Length + idRangeOffset / 2 + c - start];
						}

						yield return (c, id);
					}
				}
			}
		}
	}

	public class Subtable6 : Subtable {
		public ushort FirstCode;
		public ushort EntryCount;
		[Size(nameof(EntryCount))]
		public ushort[] GlyphIdArray = null!;

		public override IEnumerable<(ushort charcode, GlyphId id)> Glyphs {
			get {
				for ( int i = 0; i < EntryCount; i++ ) {
					if ( GlyphIdArray[i] != 0)
						yield return ((ushort)(i + FirstCode), new GlyphId(GlyphIdArray[i]));
				}
			}
		}
	}
}