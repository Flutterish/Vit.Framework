using Vit.Framework.Parsing.Binary;
using Vit.Framework.Parsing;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public struct OffsetSize {
	public byte Size;

	public override string ToString () {
		return $"{Size * 8}";
	}
}


public struct Offset {
	[ParseWith( nameof( getValue ) )]
	public long Value;

	static long getValue ( [Resolve] OffsetSize offsetSize, EndianCorrectingBinaryReader reader ) {
		if ( offsetSize.Size == 1 )
			return reader.Read<byte>();
		if ( offsetSize.Size == 2 )
			return reader.Read<ushort>();
		if ( offsetSize.Size == 4 )
			return reader.Read<uint>();

		return -1;
	}

	public static implicit operator long ( Offset offset ) {
		return offset.Value;
	}

	public override string ToString () {
		return $"0x{Value:x}";
	}
}
