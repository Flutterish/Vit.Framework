using System.Diagnostics;
using System.Text;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class CffTable_old : Table {
	public byte Major;
	public byte Minor;
	public byte HeaderSize;
	public OffsetSize Offset0Size;

	[Size(nameof(headerPaddingSize))]
	public byte[] headerPadding = null!;

	static int headerPaddingSize ( byte headerSize ) {
		return headerSize - 4;
	}

	public Index<string> NameIndex;
	public Index<TopDict> TopDictIndex;
	public Index<string> StringIndex;
	public Index<CharString> GlobalSubrs;

	[ParseWith( nameof( parseEncodings ) )]
	public List<EncodingTable?> Encodings = null!;
	static List<EncodingTable?> parseEncodings ( Index<TopDict> topDictIndex ) {
		var data = new List<EncodingTable?>( topDictIndex.Count );
		foreach ( var i in topDictIndex.Data ) {
			data.Add( null );
		}
		return data;
	}

	[ParseWith( nameof( parseCharStrings ) )]
	public List<Index<CharString>?> CharStrings = null!;
	static List<Index<CharString>?> parseCharStrings ( Index<TopDict> topDictIndex, BinaryFileParser.Context context ) {
		var data = new List<Index<CharString>?>( topDictIndex.Count );
		foreach ( var i in topDictIndex.Data ) {
			var offset = (long)(double)( i.Entries.GetValueOrDefault( TopDict.Key.CharStrings ) ?? 0d );
			if ( offset == 0 )
				data.Add( null );
			else {
				var pos = context.Reader.Stream.Position;
				context.Reader.Stream.Position = context.Offset + offset;
				data.Add( BinaryFileParser.Parse<Index<CharString>>( context ) );
				context.Reader.Stream.Position = pos;
			}
		}
		return data;
	}

	[ParseWith( nameof( parseCharsets ) )]
	public List<CharsetTable?> Charsets = null!;
	static List<CharsetTable?> parseCharsets ( Index<TopDict> topDictIndex, List<Index<CharString>?> charStrings, BinaryFileParser.Context context ) {
		var data = new List<CharsetTable?>( topDictIndex.Count );
		for ( int j = 0; j < topDictIndex.Data.Length; j++ ) {
			var i = topDictIndex.Data[j];
			var offset = (long)(double)( i.Entries.GetValueOrDefault( TopDict.Key.Charset ) ?? 0d );
			if ( offset == 0 )
				data.Add( null );
			else {
				var pos = context.Reader.Stream.Position;
				context.Reader.Stream.Position = context.Offset + offset;
				var ctx = context.GetChildContext( typeof( CharsetTable ) );
				ctx.Cache.Add( typeof( Index<CharString> ), charStrings[j]!.Value );
				data.Add( BinaryFileParser.Parse<CharsetTable>( ctx ) );
				context.Reader.Stream.Position = pos;
			}
		}
		return data;
	}

	[ParseWith( nameof( parsePrivate ) )]
	public List<PrivateDict?> PrivateDicts = null!;
	static List<PrivateDict?> parsePrivate ( Index<TopDict> topDictIndex, BinaryFileParser.Context context ) {
		var data = new List<PrivateDict?>( topDictIndex.Count );
		var pos = context.Reader.Stream.Position;
		for ( int j = 0; j < topDictIndex.Data.Length; j++ ) {
			var i = topDictIndex.Data[j];
			var dictData = (double[])( i.Entries.GetValueOrDefault( TopDict.Key.Private ) ?? Array.Empty<double>() );
			if ( dictData.Length == 0 )
				data.Add( null );
			else {
				var (length, offset) = ((long)dictData[0], new Offset { Value = (long)dictData[1] });
				context.Reader.Stream.Position = context.Offset + offset;
				var bin = new byte[length];
				context.Reader.Stream.Read( bin );
				data.Add( new PrivateDict( bin, context ) );
			}
		}
		context.Reader.Stream.Position = pos;
		return data;
	}

	public string GetString ( SID id ) {
		if ( StandardStrings.TryGetValue( id, out var str ) )
			return str;

		return StringIndex.Data[id.Id - StandardStrings.Count];
	}

	public static readonly Dictionary<SID, string> StandardStrings = new() {
		{0, ".notdef" }, {1, "space" }, {2, "exclam" }, {3, "quotedbl" }, {4, "numbersign" }, {5, "dollar" }, {6, "percent" }, {7, "ampersand" },
		{8, "quoteright" }, {9, "parenleft" }, {10, "parenright" }, {11, "asterisk" }, {12, "plus" }, {13, "comma" }, {14, "hyphen" }, {15, "period" },
		{16, "slash" }, {17, "zero" }, {18, "one" }, {19, "two" }, {20, "three" }, {21, "four" }, {22, "five" }, {23, "six" },
		{24, "seven" }, {25, "eight" }, {26, "nine" }, {27, "colon" }, {28, "semicolon" }, {29, "less" }, {30, "equal" }, {31, "greater" },
		{32, "question" }, {33, "at" }, {34, "A" }, {35, "B" }, {36, "C" }, {37, "D" }, {38, "E" }, {39, "F" },
		{40, "G" }, {41, "H" }, {42, "I" }, {43, "J" }, {44, "K" }, {45, "L" }, {46, "M" }, {47, "N" },
		{48, "O" }, {49, "P" }, {50, "Q" }, {51, "R" }, {52, "S" }, {53, "T" }, {54, "U" }, {55, "V" },
		{56, "W" }, {57, "X" }, {58, "Y" }, {59, "Z" }, {60, "bracketleft" }, {61, "backslash" }, {62, "bracketright" }, {63, "asciicircum" },
		{64, "underscore" }, {65, "quoteleft" }, {66, "a" }, {67, "b" }, {68, "c" }, {69, "d" }, {70, "e" }, {71, "f" },
		{72, "g" }, {73, "h" }, {74, "i" }, {75, "j" }, {76, "k" }, {77, "l" }, {78, "m" }, {79, "n" },
		{80, "o" }, {81, "p" }, {82, "q" }, {83, "r" }, {84, "s" }, {85, "t" }, {86, "u" }, {87, "v" },
		{88, "w" }, {89, "x" }, {90, "y" }, {91, "z" }, {92, "braceleft" }, {93, "bar" }, {94, "braceright" }, {95, "asciitilde" },
		{96, "exclamdown" }, {97, "cent" }, {98, "sterling" }, {99, "fraction" }, {100, "yen" }, {101, "florin" }, {102, "section" }, {103, "currency" },
		{104, "quotesingle" }, {105, "quotedblleft" }, {106, "guillemotleft" }, {107, "guilsinglleft" }, {108, "guilsinglright" }, {109, "fi" }, {110, "fl" }, {111, "endash" },
		{112, "dagger" }, {113, "daggerdbl" }, {114, "periodcentered" }, {115, "paragraph" }, {116, "bullet" }, {117, "quotesinglbase" }, {118, "quotedblbase" }, {119, "quotedblright" },
		{120, "guillemotright" }, {121, "ellipsis" }, {122, "perthousand" }, {123, "questiondown" }, {124, "grave" }, {125, "acute" }, {126, "circumflex" }, {127, "tilde" },
		{128, "macron" }, {129, "breve" }, {130, "dotaccent" }, {131, "dieresis" }, {132, "ring" }, {133, "cedilla" }, {134, "hungarumlaut" }, {135, "ogonek" },
		{136, "caron" }, {137, "emdash" }, {138, "AE" }, {139, "ordfeminine" }, {140, "Lslash" }, {141, "Oslash" }, {142, "OE" }, {143, "ordmasculine" },
		{144, "ae" }, {145, "dotlessi" }, {146, "lslash" }, {147, "oslash" }, {148, "oe" }, {149, "germandbls" }, {150, "onesuperior" }, {151, "logicalnot" },
		{152, "mu" }, {153, "trademark" }, {154, "Eth" }, {155, "onehalf" }, {156, "plusminus" }, {157, "Thorn" }, {158, "onequarter" }, {159, "divide" },
		{160, "brokenbar" }, {161, "degree" }, {162, "thorn" }, {163, "threequarters" }, {164, "twosuperior" }, {165, "registered" }, {166, "minus" }, {167, "eth" },
		{168, "multiply" }, {169, "threesuperior" }, {170, "copyright" }, {171, "Aacute" }, {172, "Acircumflex" }, {173, "Adieresis" }, {174, "Agrave" }, {175, "Aring" },
		{176, "Atilde" }, {177, "Ccedilla" }, {178, "Eacute" }, {179, "Ecircumflex" }, {180, "Edieresis" }, {181, "Egrave" }, {182, "Iacute" }, {183, "Icircumflex" },
		{184, "Idieresis" }, {185, "Igrave" }, {186, "Ntilde" }, {187, "Oacute" }, {188, "Ocircumflex" }, {189, "Odieresis" }, {190, "Ograve" }, {191, "Otilde" },
		{192, "Scaron" }, {193, "Uacute" }, {194, "Ucircumflex" }, {195, "Udieresis" }, {196, "Ugrave" }, {197, "Yacute" }, {198, "Ydieresis" }, {199, "Zcaron" },
		{200, "aacute" }, {201, "acircumflex" }, {202, "adieresis" }, {203, "agrave" }, {204, "aring" }, {205, "atilde" }, {206, "ccedilla" }, {207, "eacute" },
		{208, "ecircumflex" }, {209, "edieresis" }, {210, "egrave" }, {211, "iacute" }, {212, "icircumflex" }, {213, "idieresis" }, {214, "igrave" }, {215, "ntilde" },
		{216, "oacute" }, {217, "ocircumflex" }, {218, "odieresis" }, {219, "ograve" }, {220, "otilde" }, {221, "scaron" }, {222, "uacute" }, {223, "ucircumflex" },
		{224, "udieresis" }, {225, "ugrave" }, {226, "yacute" }, {227, "ydieresis" }, {228, "zcaron" }, {229, "exclamsmall" }, {230, "Hungarumlautsmall" }, {231, "dollaroldstyle" },
		{232, "dollarsuperior" }, {233, "ampersandsmall" }, {234, "Acutesmall" }, {235, "parenleftsuperior" }, {236, "parenrightsuperior" }, {237, "twodotenleader" }, {238, "onedotenleader" }, {239, "zerooldstyle" },
		{240, "oneoldstyle" }, {241, "twooldstyle" }, {242, "threeoldstyle" }, {243, "fouroldstyle" }, {244, "fiveoldstyle" }, {245, "sixoldstyle" }, {246, "sevenoldstyle" }, {247, "eightoldstyle" },
		{248, "nineoldstyle" }, {249, "commasuperior" }, {250, "threequartersemdash" }, {251, "periodsuperior" }, {252, "questionsmall" }, {253, "asuperior" }, {254, "bsuperior" }, {255, "centsuperior" },
		{256, "dsuperior" }, {257, "esuperior" }, {258, "isuperior" }, {259, "lsuperior" }, {260, "msuperior" }, {261, "nsuperior" }, {262, "osuperior" }, {263, "rsuperior" },
		{264, "ssuperior" }, {265, "tsuperior" }, {266, "ff" }, {267, "ffi" }, {268, "ffl" }, {269, "parenleftinferior" }, {270, "parenrightinferior" }, {271, "Circumflexsmall" },
		{272, "hyphensuperior" }, {273, "Gravesmall" }, {274, "Asmall" }, {275, "Bsmall" }, {276, "Csmall" }, {277, "Dsmall" }, {278, "Esmall" }, {279, "Fsmall" },
		{280, "Gsmall" }, {281, "Hsmall" }, {282, "Ismall" }, {283, "Jsmall" }, {284, "Ksmall" }, {285, "Lsmall" }, {286, "Msmall" }, {287, "Nsmall" },
		{288, "Osmall" }, {289, "Psmall" }, {290, "Qsmall" }, {291, "Rsmall" }, {292, "Ssmall" }, {293, "Tsmall" }, {294, "Usmall" }, {295, "Vsmall" },
		{296, "Wsmall" }, {297, "Xsmall" }, {298, "Ysmall" }, {299, "Zsmall" }, {300, "colonmonetary" }, {301, "onefitted" }, {302, "rupiah" }, {303, "Tildesmall" },
		{304, "exclamdownsmall" }, {305, "centoldstyle" }, {306, "Lslashsmall" }, {307, "Scaronsmall" }, {308, "Zcaronsmall" }, {309, "Dieresissmall" }, {310, "Brevesmall" }, {311, "Caronsmall" },
		{312, "Dotaccentsmall" }, {313, "Macronsmall" }, {314, "figuredash" }, {315, "hypheninferior" }, {316, "Ogoneksmall" }, {317, "Ringsmall" }, {318, "Cedillasmall" }, {319, "questiondownsmall" },
		{320, "oneeighth" }, {321, "threeeighths" }, {322, "fiveeighths" }, {323, "seveneighths" }, {324, "onethird" }, {325, "twothirds" }, {326, "zerosuperior" }, {327, "foursuperior" },
		{328, "fivesuperior" }, {329, "sixsuperior" }, {330, "sevensuperior" }, {331, "eightsuperior" }, {332, "ninesuperior" }, {333, "zeroinferior" }, {334, "oneinferior" }, {335, "twoinferior" },
		{336, "threeinferior" }, {337, "fourinferior" }, {338, "fiveinferior" }, {339, "sixinferior" }, {340, "seveninferior" }, {341, "eightinferior" }, {342, "nineinferior" }, {343, "centinferior" },
		{344, "dollarinferior" }, {345, "periodinferior" }, {346, "commainferior" }, {347, "Agravesmall" }, {348, "Aacutesmall" }, {349, "Acircumflexsmall" }, {350, "Atildesmall" }, {351, "Adieresissmall" },
		{352, "Aringsmall" }, {353, "AEsmall" }, {354, "Ccedillasmall" }, {355, "Egravesmall" }, {356, "Eacutesmall" }, {357, "Ecircumflexsmall" }, {358, "Edieresissmall" }, {359, "Igravesmall" },
		{360, "Iacutesmall" }, {361, "Icircumflexsmall" }, {362, "Idieresissmall" }, {363, "Ethsmall" }, {364, "Ntildesmall" }, {365, "Ogravesmall" }, {366, "Oacutesmall" }, {367, "Ocircumflexsmall" },
		{368, "Otildesmall" }, {369, "Odieresissmall" }, {370, "OEsmall" }, {371, "Oslashsmall" }, {372, "Ugravesmall" }, {373, "Uacutesmall" }, {374, "Ucircumflexsmall" }, {375, "Udieresissmall" },
		{376, "Yacutesmall" }, {377, "Thornsmall" }, {378, "Ydieresissmall" }, {379, "001.000" }, {380, "001.001" }, {381, "001.002" }, {382, "001.003" }, {383, "Black" },
		{384, "Bold" }, {385, "Book" }, {386, "Light" }, {387, "Medium" }, {388, "Regular" }, {389, "Roman" }, {390, "Semibold" }
	};

	[Cache]
	public struct OffsetSize {
		public byte Size;

		public override string ToString () {
			return $"{Size * 8}";
		}
	}

	public struct Offset {
		[ParseWith(nameof(getValue))]
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

	public struct SID {
		public byte A;
		public byte B;
		public ushort Id => (ushort)(A<<8 | B);

		public SID ( ushort id ) {
			A = (byte)(id >> 8);
			B = (byte)(id & 0xff);
		}

		public static implicit operator SID ( int id )
			=> new( (ushort)id );

		public override string ToString () {
			if ( StandardStrings.TryGetValue( this, out var std ) )
				return $"Standard String ID {A<<8 | B} \"{std}\"";
			return $"String ID {A << 8 | B}";
		}
	}

	public struct Index<T> {
		public ushort Count;
		public OffsetSize OffsetSize;
		[Size(nameof(getCount))]
		public Offset[] Offsets;

		[ParseWith(nameof(getData))]
		public T[] Data;

		static int getCount ( ushort count ) {
			return count + 1;
		}

		static T[] getData ( Offset[] offsets, BinaryFileParser.Context context ) {
			var count = offsets.Length - 1;
			var data = new T[count];
			long index = offsets[0].Value;
			context.Reader.Stream.Position += index - 1;
			for ( int i = 1; i < offsets.Length; i++ ) {
				var nextIndex = offsets[i].Value;
				var length = nextIndex - index;
				index = nextIndex;

				if ( typeof(T) == typeof(string) ) {
					var raw = new byte[length];
					context.Reader.Stream.Read( raw.AsSpan() );
					data[i - 1] = (T)(object)Encoding.UTF8.GetString( raw );
				}
				else if ( typeof(T).IsAssignableTo( typeof(Dict) ) ) {
					var raw = new byte[length];
					context.Reader.Stream.Read( raw.AsSpan() );

					if ( typeof(T).IsAssignableTo( typeof(TopDict) ) )
						data[i - 1] = (T)(object)new TopDict( raw );
					else
						data[i - 1] = (T)(object)new Dict( raw );
				}
				else if ( typeof( T ).IsAssignableTo( typeof( CharString ) ) ) {
					var raw = new byte[length];
					context.Reader.Stream.Read( raw.AsSpan() );

					data[i - 1] = (T)(object)new CharString( raw );
				}
				else {
					data[i - 1] = BinaryFileParser.Parse<T>( context );
				}
			}

			return data;
		}
	}

	public class Dict {
		public (byte[] key, byte[][] values)[] Data;

		public override string ToString () {
			StringBuilder sb = new();
			sb.AppendLine( "{" );
			foreach ( var (key, values) in Data ) {
				sb.Append( "[" );
				sb.Append( string.Join( " ", key.Select( x => $"{x}" ) ) );
				sb.Append( "] = <" );
				sb.Append( string.Join( ", ", values.Select( y => string.Join( " ", y.Select( x => $"{x:x}" ) ) ) ) );
				sb.AppendLine( ">" );
			}
			sb.Append( "}" );
			return sb.ToString();
		}

		public Dict ( ReadOnlySpan<byte> raw ) {
			var x = raw.ToArray();
			List<(byte[] key, byte[][] value)> pairs = new();
			while ( raw.Length != 0 ) {
				List<byte[]> values = new();

				var b0 = raw[0];
				while ( b0 > 21 ) {
					var type = b0 switch {
						28 => 3,
						29 => 4,
						<= 246 => 0,
						<= 250 => 1,
						_ => 2
					};
					var size = type switch {
						0 => 1,
						1 or 2 => 2,
						3 => 3,
						_ => 5
					};
					var value = raw.Slice( 0, size );
					values.Add( value.ToArray() );
					raw = raw.Slice( value.Length );

					b0 = raw[0];
				}

				var key = raw.Slice( 0, 1 );
				if ( key is [12] )
					key = raw.Slice( 0, 2 );

				pairs.Add( (key.ToArray(), values.ToArray()) );
				raw = raw.Slice( key.Length );
			}

			Data = pairs.ToArray();
		}

		public SID ParseSid ( byte[][] data ) {
			return ParseSid( data[0] );
		}
		public SID ParseSid ( byte[] data ) {
			return new( (ushort)ParseNumber( data ) );
		}

		public double[] ParseArray ( byte[][] data ) {
			var all = new double[data.Length];
			for ( int i = 0; i < data.Length; i++ ) {
				all[i] = ParseNumber( data[i] );
			}
			return all;
		}

		public double ParseNumber ( byte[][] data ) {
			return ParseNumber( data[0] );
		}
		public double ParseNumber ( byte[] data ) {
			if ( data[0] is >= 32 and <= 246 )
				return data[0] - 139;
			if ( data[0] is >= 246 and <= 250 )
				return ( data[0] - 247 ) * 256 + data[1] + 108;
			if ( data[0] is >= 251 and <= 254 )
				return -( data[0] - 251 ) * 256 - data[1] - 108;
			if ( data[0] is 28 )
				return (data[1] << 8) | data[2];
			if ( data[0] is 29 )
				return (data[1] << 24) | (data[2] << 16) | (data[3] << 8) | (data[4]);

			return double.NaN;
 		}
	}

	public class TopDict : Dict {
		public readonly Dictionary<Key, object> Entries = new();
		public TopDict ( ReadOnlySpan<byte> raw ) : base( raw ) {
			foreach ( var (key, values) in Data ) {
				Key entryKey;
				if ( key.Length == 2 )
					entryKey = (Key)( key[0] << 8 | key[1] );
				else
					entryKey = (Key)key[0];

				Entries[entryKey] = entryKey switch {
					Key.Version or Key.Notice or Key.Copyright or Key.FullName or Key.FailyName or Key.Weight
						=> ParseSid( values ),
					Key.FontBBox => ParseArray( values ),
					Key.Charset or Key.CharStrings
						=> ParseNumber( values ),
					Key.Private => ParseArray( values ),
					_ => values
				};
			}
		}

		public enum Key {
			Version = 0,
			Notice = 1,
			Copyright = 12 << 8 | 0,
			FullName = 2,
			FailyName = 3,
			Weight = 4,
			FontBBox = 5,
			Charset = 15,
			CharStrings = 17,
			Private = 18
		}
	}

	public class PrivateDict : Dict {
		public readonly Dictionary<Key, object> Entries = new();
		public Index<CharString>? Subrs;
		public PrivateDict ( ReadOnlySpan<byte> raw, BinaryFileParser.Context context ) : base( raw ) {
			foreach ( var (key, values) in Data ) {
				Key entryKey;
				if ( key.Length == 2 )
					entryKey = (Key)( key[0] << 8 | key[1] );
				else
					entryKey = (Key)key[0];

				Entries[entryKey] = entryKey switch {
					Key.Subrs or Key.DefaultWidthX or Key.NominalWidthX => ParseNumber( values ),
					Key.BlueValues or Key.OtherBlues => ParseArray( values ),
					_ => values
				};
			}

			if ( Entries.TryGetValue( Key.Subrs, out var offsetValue ) ) {
				var offset = (long)(double)offsetValue;
				var pos = context.Reader.Stream.Position;
				context.Reader.Stream.Position += offset - raw.Length;
				Subrs = BinaryFileParser.Parse<Index<CharString>>( context );
				context.Reader.Stream.Position = pos;
			}
		}

		public enum Key {
			Subrs = 19,
			BlueValues = 6,
			OtherBlues = 7,
			DefaultWidthX = 20,
			NominalWidthX = 21
		}
	}

	public class EncodingTable {
		public byte Format;
	}

	public class CharsetTable {
		public byte Format;
		[Size(nameof(getSize))]
		public SID[] Glyphs = null!;

		static int getSize ( [Resolve] Index<CharString> charStrings ) {
			return charStrings.Count - 1;
		}
	}

	public class CharString {
		public byte[] Data;
		public CharString ( byte[] raw ) {
			Data = raw;
		}
	}

	public class CharStringEvaluator {
		Index<CharString> globals;
		int globalBias;
		Index<CharString> locals;
		int localBias;
		public CharStringEvaluator ( Index<CharString> globals, Index<CharString>? locals ) {
			this.globals = globals;
			this.locals = locals ?? globals;

			globalBias = getBias( this.globals.Count );
			localBias = getBias( this.locals.Count );
		}

		int getBias ( int count ) {
			if ( count < 1240 )
				return 107;
			if ( count < 33900 )
				return 1131;
			return 32768;
		}

		class Context {
			public List<string> Explain = new();
			public List<string> SVG = new(); // TODO remove - debug only
			public Point2<double> Position;
			public Stack<double> ArgumentStack = new();
			public double? Width;
			public bool IsWidthPending = true;
			public string? LastHintType;
			public bool AreHintsSet;
			public List<(double from, double to)> HorizontalHints = new();
			public List<(double from, double to)> VerticalHints = new();

			public Outline<double> Outline;
			public Spline2<double>? Spline;

			public Context ( Outline<double> outline ) {
				Outline = outline;
			}

			public void Begin ( double dx, double dy ) {
				if ( Spline != null )
					Finish();

				Position += new Vector2<double>( dx, dy );
				SVG.Add( $"Move to {Position}" );

				Spline = new( Position );
			}

			public void Finish () {
				SVG.Add( "Close curve" );

				if ( Spline != null ) {
					if ( Spline.Points[^1] != Spline.Points[0] )
						Spline.AddLine( Spline.Points[0] );
					Outline.Splines.Add( Spline );
				}
				Spline = null;
			}

			public void AddCubicBezier ( double dxa, double dya, double dxb, double dyb, double dxc, double dyc, bool flip = false ) {
				var a = Position;
				var b = a + (flip ? new Vector2<double>( dya, dxa ) : new Vector2<double>( dxa, dya ));
				var c = b + new Vector2<double>( dxb, dyb );
				var d = c + (flip ? new Vector2<double>( dyc, dxc ) : new Vector2<double>( dxc, dyc ));
				Position = d;
				SVG.Add( $"Bezier curve from {a} through {b} and {c} to {d}" );

				Spline!.AddCubicBezier( b, c, d );
			}

			public void AddLine ( double dx, double dy, bool flip = false ) {
				var a = Position;
				var b = a + (flip ? new Vector2<double>( dy, dx ) : new Vector2<double>( dx, dy ));
				Position = b;
				SVG.Add( $"Line from {a} to {b}" );

				Spline!.AddLine( b );
			}
		}
		public void Evaluate ( CharString str, Outline<double> outline ) {
			Context context = new( outline );
			ReadOnlySpan<byte> data = str.Data;
			evaluate( context, ref data );
		}

		void evaluate ( Context context, ref ReadOnlySpan<byte> data ) {
			while ( data.Length != 0 ) {
				if ( tryConsumeNumber( ref data, out var number ) ) {
					context.Explain.Add( $"Push `{number}`" );
					context.ArgumentStack.Push( number );
				}
				else {
					consumeOperator( context, ref data );
				}
			}
		}

		bool tryConsumeNumber ( ref ReadOnlySpan<byte> raw, out double value ) {
			if ( raw[0] is 28 or >= 32 ) {
				value = consumeNumber( ref raw );
				return true;
			}

			value = double.NaN;
			return false;
		}

		double consumeNumber ( ref ReadOnlySpan<byte> raw ) {
			var data = raw;
			if ( data[0] is >= 32 and <= 246 ) {
				raw = raw[1..];
				return data[0] - 139;
			}
			if ( data[0] is >= 246 and <= 250 ) {
				raw = raw[2..];
				return ( data[0] - 247 ) * 256 + data[1] + 108;
			}
			if ( data[0] is >= 251 and <= 254 ) {
				raw = raw[2..];
				return -( data[0] - 251 ) * 256 - data[1] - 108;
			}
			if ( data[0] is 255 ) {
				raw = raw[5..];
				return (double)BitConverter.ToInt32( data[1..5] ) / ( 1 << 16 );
			}
			if ( data[0] is 28 ) {
				raw = raw[3..];
				return BitConverter.ToInt16( data[1..3] );
			}

			return double.NaN;
		}

		bool tryConsumeOperator ( Context context, ref ReadOnlySpan<byte> raw ) {
			if ( raw[0] is not 28 and <= 31 ) {
				consumeOperator( context, ref raw );
				return true;
			}

			return false;
		}

		void consumeOperator ( Context context, ref ReadOnlySpan<byte> raw ) {
			var b0 = raw[0];
			raw = raw[1..];

			if ( b0 is 14 ) { // end char
				pushWidth( context );
				context.Explain.Add( "End character" );
				context.Finish();
			}
			else if ( b0 is 1 or 3 or 18 or 23 ) { // hints
				Debug.Assert( !context.AreHintsSet );
				pushHints( context, b0 is 1 or 18 ? "horizontal" : "vertical", @implicit: false );
			}
			else if ( b0 is 19 or 20 ) { // hint masks
				if ( !context.AreHintsSet )
					pushHints( context, context.LastHintType == "horizontal" ? "vertical" : "horizontal", @implicit: true );

				pushWidth( context );
				var count = context.VerticalHints.Count + context.HorizontalHints.Count;
				var size = ( count + 7 ) / 8;
				var mask = raw[..size];
				raw = raw[size..];
				context.Explain.Add( $"Set hint {(b0 == 20 ? "counter" : "")}mask ({count} hint(s)) to {Convert.ToHexString(mask)}" );
				context.AreHintsSet = true;
			}
			else if ( b0 is 21 or 22 or 4 ) { // move to
				var args = getArgs( context, b0 is 21 ? 2 : 1 );
				var dx = b0 is 21 or 22 ? args[0] : 0;
				var dy = b0 is 4 ? args[0] : b0 is 21 ? args[1] : 0;
				pushWidth( context );
				context.Explain.Add( $"Move to" );
				context.Begin( dx, dy );
			}
			else if ( b0 is 5 ) { // lines
				var count = context.ArgumentStack.Count / 2;
				Debug.Assert( count >= 1 );
				var vargs = getArgs( context, count * 2 );
				context.Explain.Add( $"Line to ({count} time(s))" );

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( i * 2, 2 );
					context.AddLine( args[0], args[1] );
				}
			}
			else if ( b0 is 6 or 7 ) {
				var count = context.ArgumentStack.Count;
				Debug.Assert( count >= 1 );
				var vargs = getArgs( context, count );
				context.Explain.Add( $"Line to [{b0}] ({count} time(s))" );

				var flip = b0 is 7;
				for ( int i = 0; i < count; i++ ) {
					context.AddLine( vargs[i], 0, flip );
					flip = !flip;
				}
			}
			else if ( b0 is 8 ) { // bezier curves
				var count = context.ArgumentStack.Count / 6;
				Debug.Assert( count >= 1 );
				var vargs = getArgs( context, count * 6 );
				context.Explain.Add( $"Bezier curve ({count} time(s))" );

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( i * 6, 6 );
					context.AddCubicBezier( 
						args[0], args[1],
						args[2], args[3], 
						args[4], args[5] 
					);
				}
			}
			else if ( b0 is 26 or 27 ) {
				var count = context.ArgumentStack.Count / 4;
				var parity = context.ArgumentStack.Count % 4;
				Debug.Assert( count >= 1 && parity is 0 or 1 );
				var vargs = getArgs( context, parity + count * 4 );
				context.Explain.Add( $"Bezier curve [{b0}] ({count} time(s))" );

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( parity + i * 4, 4 );
					var dxa = i == 0 && parity == 1 ? vargs[0] : 0;
					context.AddCubicBezier( 
						dxa, args[0], 
						args[1], args[2], 
						0, args[3],
						flip: b0 is 27
					);
				}
			}
			else if ( b0 is 30 or 31 ) {
				var parity = context.ArgumentStack.Count % 8;
				var count = context.ArgumentStack.Count / 8;
				Debug.Assert( ( parity is 0 or 1 && count >= 1 ) || ( parity is 4 or 5 ) );
				var vargs = getArgs( context, parity + count * 8 );
				context.Explain.Add( $"Bezier curve [{b0}] ({( parity < 2 ? count : ( count + 1 ) )} time(s))" );

				bool flip = b0 == 31;
				int offset = 0;

				if ( parity >= 2 ) {
					var dy = count == 0 && parity == 5 ? vargs[^1] : 0;
					context.AddCubicBezier(
						0, vargs[0],
						vargs[1], vargs[2],
						vargs[3], dy,
						flip
					);
					offset = 4;
					flip = !flip;
				}

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( offset + i * 8, 8 );
					context.AddCubicBezier(
						0, args[0],
						args[1], args[2],
						args[3], 0,
						flip
					);

					var dx = i == (count - 1) && parity == (1 + offset) ? vargs[^1] : 0;
					context.AddCubicBezier(
						args[4], 0,
						args[5], args[6],
						dx, args[7],
						flip
					);
				}
			}
			else if ( b0 is 24 ) { // combinations
				var count = (context.ArgumentStack.Count - 2) / 6;
				Debug.Assert( count >= 1 && context.ArgumentStack.Count % 6 == 2 );
				var vargs = getArgs( context, count * 6 + 2 );
				context.Explain.Add( $"Bezier curve ({count} time(s)) + line to" );

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( i * 6, 6 );
					context.AddCubicBezier(
						args[0], args[1],
						args[2], args[3],
						args[4], args[5]
					);
				}
				context.AddLine( vargs[^2], vargs[^1] );
			}
			else if ( b0 is 25 ) { 
				var count = ( context.ArgumentStack.Count - 6 ) / 2;
				Debug.Assert( context.ArgumentStack.Count >= 8 && context.ArgumentStack.Count % 2 == 0 );
				var vargs = getArgs( context, count * 2 + 6 );
				context.Explain.Add( $"Line to ({count} time(s)) + bezier curve" );

				for ( int i = 0; i < count; i++ ) {
					var args = vargs.AsSpan().Slice( i * 2, 2 );
					context.AddLine( args[0], args[1] );
				}
				context.AddCubicBezier(
					vargs[^6], vargs[^5],
					vargs[^4], vargs[^3],
					vargs[^2], vargs[^1]
				);
			}
			else if ( b0 is 10 ) { // call local subroutine
				var arg = (int)getArgs( context, 1 )[0] + localBias;
				context.Explain.Add( $"(start local subroutine {arg})" );
				ReadOnlySpan<byte> subr = locals.Data[arg].Data;
				evaluate( context, ref subr );
				context.Explain.Add( $"(end local subroutine {arg})" );
			}
			else if ( b0 is 29 ) { // call global subroutine
				var arg = (int)getArgs( context, 1 )[0] + globalBias;
				context.Explain.Add( $"(start global subroutine {arg})" );
				ReadOnlySpan<byte> subr = globals.Data[arg].Data;
				evaluate( context, ref subr );
				context.Explain.Add( $"(end global subroutine {arg})" );
			}
			else if ( b0 is 11 ) { // return
				context.Explain.Add( $"(return)" );
			}
			else {
				throw new NotImplementedException( "oops" );
			}
		}

		double[] getArgs ( Context context, int count ) {
			var args = new double[count];
			for ( int i = 0; i < count; i++ ) {
				args[count - 1 - i] = context.ArgumentStack.Pop();
			}

			return args;
		}

		void pushHints ( Context context, string type, bool @implicit ) {
			var count = context.ArgumentStack.Count / 2;
			getArgs( context, count * 2 );
			pushWidth( context );

			context.Explain.Add( $"{(@implicit ? "Implicitly add" : "Add" )} {count} {type} hint(s)" );
			var list = type == "horizontal" ? context.HorizontalHints : context.VerticalHints;
			for ( int i = 0; i < count; i++ ) {
				list.Add( (0, 0) ); // TODO
			}
			context.LastHintType = type;

		}
		
		void pushWidth ( Context context ) {
			if ( !context.IsWidthPending )
				return;

			context.IsWidthPending = false;
			if ( context.ArgumentStack.TryPop( out var width ) ) {
				context.Explain.Add( $"Width = {width}" );
				context.Width = width;
			}
		}
	}
}
