using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public struct CharString {
	[Size( nameof( getSize ) )]
	public BinaryArrayView<byte> Data;

	static int getSize ( [Resolve] int length ) => length;
}
