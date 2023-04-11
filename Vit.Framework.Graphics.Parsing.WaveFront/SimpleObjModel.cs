using System.Globalization;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.Parsing.WaveFront;

public class SimpleObjModel {
	public readonly List<ObjVertex> Vertices = new();
	public readonly List<uint> Indices = new();
	private SimpleObjModel () { }

	public static SimpleObjModel FromText ( string data )
		=> FromLines( data.ReplaceLineEndings( "\n" ).Split( '\n' ) );

	public static SimpleObjModel FromLines ( IEnumerable<string> lines ) {
		SimpleObjModel model = new();

		static float parseFloat ( ReadOnlySpan<char> data ) {
			return float.Parse( data, CultureInfo.InvariantCulture );
		}
		static int parseInt ( ReadOnlySpan<char> data ) {
			return int.Parse( data, CultureInfo.InvariantCulture );
		}
		static bool next ( ref ReadOnlySpan<char> chars, out ReadOnlySpan<char> data, char separator = ' ' ) {
			while ( chars.Length > 0 && chars[0] == ' ' )
				chars = chars[1..];

			var nextIndex = chars.IndexOf( separator );
			if ( nextIndex == -1 ) {
				data = chars;
				chars = default;
				return data.Length != 0;
			}

			data = chars[..nextIndex];
			chars = chars[( nextIndex + 1 )..];

			return true;
		}
		static ReadOnlySpan<char> requiredNext ( ref ReadOnlySpan<char> chars, char separator = ' ' ) {
			if ( !next( ref chars, out var data, separator ) )
				throw new InvalidDataException( "Obj file was malformed" );

			return data;
		}
		static ReadOnlySpan<char> optionalNext ( ref ReadOnlySpan<char> chars, string @default, char separator = ' ' ) {
			if ( !next( ref chars, out var data, separator ) )
				return @default;

			return data;
		}

		List<Vector4<float>> positions = new() { new(0) };
		List<Vector3<float>> textureCoords = new() { new(0) };
		List<Vector3<float>> normals = new() { new(0) };
		Dictionary<(int, int, int), uint> vertices = new();
		uint getVertex ( int pos, int uv, int normal ) {
			pos = pos < 0 ? ( positions.Count + pos ) : pos;
			uv = uv < 0 ? ( textureCoords.Count + uv ) : uv;
			normal = normal < 0 ? ( normals.Count + normal ) : normal;
			
			var key = (pos, uv, normal);
			if ( !vertices.TryGetValue( key, out var index ) ) {
				model.Vertices.Add( new() {
					Position = positions[pos],
					TextureCoordinates = textureCoords[uv],
					Normal = normals[normal]
				} );

				vertices.Add( key, index = (uint)model.Vertices.Count - 1 );
			}

			return index;
		}
		uint parseFaceIndex ( ref ReadOnlySpan<char> chars ) {
			var data = requiredNext( ref chars ); 

			var pos = parseInt( requiredNext( ref data, '/' ) );
			var uv = 0;
			var normal = 0;
			if ( data.Length != 0 ) {
				var uvData = requiredNext( ref data, '/' );
				if ( uvData.Length != 0 )
					uv = parseInt( uvData );
			}
			if ( data.Length != 0 ) {
				normal = parseInt( requiredNext( ref data, '/' ) );
			}

			return getVertex( pos, uv, normal );
		}

		foreach ( var stringLine in lines ) {
			var line = stringLine.AsSpan();

			if ( !next( ref line, out var data ) )
				continue;
			if ( data[0] == '#' )
				continue;

			if ( data.SequenceEqual( "v" ) ) {
				positions.Add( new() {
					X = parseFloat( requiredNext( ref line ) ),
					Y = parseFloat( requiredNext( ref line ) ),
					Z = parseFloat( requiredNext( ref line ) ),
					W = parseFloat( optionalNext( ref line, "1" ) )
				} );
			}
			else if ( data.SequenceEqual( "vn" ) ) {
				normals.Add( new() {
					X = parseFloat( requiredNext( ref line ) ),
					Y = parseFloat( requiredNext( ref line ) ),
					Z = parseFloat( requiredNext( ref line ) )
				} );
			}
			else if ( data.SequenceEqual( "vt" ) ) {
				textureCoords.Add( new() {
					X = parseFloat( requiredNext( ref line ) ),
					Y = parseFloat( optionalNext( ref line, "0" ) ),
					Z = parseFloat( optionalNext( ref line, "0" ) )
				} );
			}
			else if ( data.SequenceEqual( "f" ) ) {
				var a = parseFaceIndex( ref line );
				var b = parseFaceIndex( ref line );
				var c = parseFaceIndex( ref line );

				model.Indices.Add( a );
				model.Indices.Add( b );
				model.Indices.Add( c );
			}
		}

		return model;
	}
}
