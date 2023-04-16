using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public class CompactFontFormatData {
	public byte Major;
	public byte Minor;
	public byte HeaderSize;
	public OffsetSize OffsetSize;

	[Size( nameof( headerPaddingSize ) )]
	BinaryArrayView<byte> headerPadding;

	static int headerPaddingSize ( byte headerSize ) {
		return headerSize - 4;
	}

	public Index<string> NameIndex;
	public Index<TopDict> TopDictIndex;
	public Index<string> StringIndex;
	public Index<CharString> GlobalSubrs;
	BinaryViewContext context;

	public Index<CharString>? GetCharStrings ( int index ) {
		var offset = (long)TopDictIndex[index].Get( TopDict.Key.CharStrings ).FirstOrDefault();
		if ( offset == 0 )
			return null;

		return BinaryView<Index<CharString>>.Parse( context with { Offset = context.OffsetAnchor + offset } );
	}

	public PrivateDict? GetPrivateDict ( int index ) {
		var data = TopDictIndex[index].Get( TopDict.Key.Private );
		if ( data.Length != 2 )
			return null;

		var size = (int)data[0];
		var offset = (long)data[1];
		if ( offset == 0 )
			return null;

		var ctx = context with { Dependenies = new() { Parent = context.Dependenies }, Offset = context.OffsetAnchor + offset };
		ctx.CacheDependency( size );
		return BinaryView<PrivateDict>.Parse( ctx );
	}

	public Charset? GetCharset ( int index ) {
		var dict = TopDictIndex[index];
		var offset = (long)dict.Get( TopDict.Key.Charset ).FirstOrDefault();
		if ( offset == 0 )
			return null;

		int size = GetCharStrings( index )!.Value.Count - 1;
		var ctx = context with { Dependenies = new() { Parent = context.Dependenies }, Offset = context.OffsetAnchor + offset };
		ctx.CacheDependency( size );
		return BinaryView<Charset>.Parse( ctx );
	}

	public string GetString ( StringId id ) {
		if ( StringId.StandardStrings.TryGetValue( id, out var str ) )
			return str;

		return StringIndex[id.Value - StringId.StandardStrings.Count];
	}
}
