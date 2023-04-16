using System.Collections;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public static class Dict {
	public static bool TryGet ( ushort key, BinaryArrayView<byte> data, out double[] value ) {
		for ( int i = 0; i < data.Length; ) {
			var begin = i;
			var count = 0;
			var b0 = data[i];
			while ( !OperatorEncoding.IsDictOperator( b0 ) ) {
				i += OperandEncoding.GetDictOperandSize( b0 );
				b0 = data[i];
				count++;
			}

			if ( OperatorEncoding.DecodeDictOperator( data, ref i ) == key ) {
				i = begin;
				value = new double[count];
				for ( int j = 0; j < count; j++ ) {
					value[j] = OperandEncoding.DecodeDictOperand( data, ref i );
				}
				return true;
			}
		}

		value = Array.Empty<double>();
		return false;
	}

	public static double[] Get ( ushort key, BinaryArrayView<byte> data, Dictionary<ushort, double[]>? fallbacks = null ) {
		if ( TryGet( key, data, out var value ) )
			return value;
		if ( fallbacks?.TryGetValue( key, out value ) == true )
			return value;
		return Array.Empty<double>();
	}

	public static IEnumerable<KeyValuePair<T, double[]>> Enumerate<T> ( BinaryArrayView<byte> data, Dictionary<ushort, double[]>? fallbacks = null ) 
		where T : struct, Enum 
	{
		foreach ( var i in Enum.GetValues<T>() ) {
			var value = Get( (ushort)(object)i, data, fallbacks );
			if ( value == Array.Empty<double>() )
				continue;

			yield return new KeyValuePair<T, double[]>( i, value );
		}
	}
}

public struct TopDict : IEnumerable<KeyValuePair<TopDict.Key, double[]>> {
	[Size(nameof(getSize))]
	BinaryArrayView<byte> data;

	static int getSize ( [Resolve] int length ) => length;

	public double[] Get ( Key key ) {
		return Dict.Get( (ushort)key, data, fallbacks );
	}

	public IEnumerator<KeyValuePair<Key, double[]>> GetEnumerator () {
		return Dict.Enumerate<Key>( data ).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator () {
		return Dict.Enumerate<Key>( data ).GetEnumerator();
	}

	public enum Key : ushort {
		Version = 0,
		Notice = 1,
		Copyright = 12 << 8 | 0,
		FullName = 2,
		FamilyName = 3,
		Weight = 4,
		IsFixedPitch = 12 << 8 | 1,
		ItalicAngle = 12 << 8 | 2,
		UnderlinePosition = 12 << 8 | 3,
		UnderlineThickness = 12 << 8 | 4,
		PaintType = 12 << 8 | 5,
		CharstringType = 12 << 8 | 6,
		FontMatrix = 12 << 8 | 7,
		UniqueId = 13,
		FontBBox = 5,
		StrokeWidth = 12 << 8 | 8,
		XUID = 14,
		Charset = 15,
		Encoding = 16,
		CharStrings = 17,
		Private = 18,
		SyntheticBase = 12 << 8 | 20,
		PostScript = 12 << 8 | 21,
		BaseFontName = 12 << 8 | 22,
		BaseFontBlend = 12 << 8 | 23
	}

	static Dictionary<ushort, double[]> fallbacks = new() {
		{ (ushort)Key.IsFixedPitch, new double[] { 0 } },
		{ (ushort)Key.ItalicAngle, new double[] { 0 } },
		{ (ushort)Key.UnderlinePosition, new double[] { -100 } },
		{ (ushort)Key.UnderlineThickness, new double[] { 50 } },
		{ (ushort)Key.PaintType, new double[] { 0 } },
		{ (ushort)Key.CharstringType, new double[] { 2 } },
		{ (ushort)Key.FontMatrix, new double[] { 0.001, 0, 0, 0.001, 0, 0 } },
		{ (ushort)Key.FontBBox, new double[] { 0, 0, 0, 0 } },
		{ (ushort)Key.StrokeWidth, new double[] { 0 } },
		{ (ushort)Key.Charset, new double[] { 0 } },
		{ (ushort)Key.Encoding, new double[] { 0 } }
	};
}

public struct PrivateDict : IEnumerable<KeyValuePair<PrivateDict.Key, double[]>> {
	BinaryViewContext context;
	[Size( nameof( getSize ) )]
	BinaryArrayView<byte> data;

	static int getSize ( [Resolve] int length ) => length;

	public double[] Get ( Key key ) {
		return Dict.Get( (ushort)key, data, fallbacks );
	}

	public IEnumerator<KeyValuePair<Key, double[]>> GetEnumerator () {
		return Dict.Enumerate<Key>( data ).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator () {
		return Dict.Enumerate<Key>( data ).GetEnumerator();
	}

	public Index<CharString> LocalSubrs {
		get {
			var offset = (long)Get( Key.Subrs ).FirstOrDefault();
			return BinaryView<Index<CharString>>.Parse( context with { Offset = context.Offset + offset } );
		}
	}

	public enum Key : ushort {
		BlueValues = 6,
		OtherBlues = 7,
		FamilyBlues = 8,
		FamilyOtherBlues = 9,
		BlueScale = 12 << 8 | 9,
		BlueShift = 12 << 8 | 10,
		BlueFuzz = 12 << 8 | 11,
		StdHW = 10,
		StdVW = 11,
		StemSnapH = 12 << 8 | 12,
		StemSnapV = 12 << 8 | 13,
		ForceBold = 12 << 8 | 14,
		LanguageGroup = 12 << 8 | 17,
		ExpansionFactor = 12 << 8 | 18,
		InitialRandomSeed = 12 << 8 | 19,
		Subrs = 19,
		DefaultWidthX = 20,
		NominalWidthX = 21
	}

	static Dictionary<ushort, double[]> fallbacks = new() {
		{ (ushort)Key.BlueScale, new double[] { 0.039625 } },
		{ (ushort)Key.BlueShift, new double[] { 7 } },
		{ (ushort)Key.BlueFuzz, new double[] { 1 } },
		{ (ushort)Key.ForceBold, new double[] { 0 } },
		{ (ushort)Key.LanguageGroup, new double[] { 0 } },
		{ (ushort)Key.ExpansionFactor, new double[] { 0.06 } },
		{ (ushort)Key.InitialRandomSeed, new double[] { 0 } },
		{ (ushort)Key.DefaultWidthX, new double[] { 0 } },
		{ (ushort)Key.NominalWidthX, new double[] { 0 } }
	};
}