using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public struct Charset {
	public byte Format;
	[Size( nameof( getSize ) )]
	public BinaryArrayView<StringId> Glyphs;

	static int getSize ( [Resolve] int length ) {
		return length;
	}
}
