using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class KeringTable : Table {
	public ushort Version;
	public ushort SubtableCount;
	[Size(nameof(SubtableCount))]
	public Subtable[] Subtables = null!;

	[TypeSelector(nameof(selectType))]
	public abstract class Subtable {
		public ushort Version;
		public ushort Length;
		public ushort Coverage;

		static Type? selectType ( ushort version ) {
			return version switch {
				0 => typeof( Subtable0 ),
				_ => null
			};
		}
	}

	public class Subtable0 : Subtable {
		public ushort PairCount;
		public ushort SearchRange;
		public ushort EntrySelector;
		public ushort RangeShift;
		[Size(nameof(PairCount))]
		public KeringPair[] KeringPairs = null!;

		public struct KeringPair {
			public ushort Left;
			public ushort Right;
			public FontWord Value;
		}
	}
}
