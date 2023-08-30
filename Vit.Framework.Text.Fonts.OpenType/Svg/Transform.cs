using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public static class Transform {
	public static Matrix3<double> Parse ( ByteString data ) {
		if ( !transformList( ref data, out var matrix ) )
			throw new InvalidDataException( "Could not parse transform" );

		return matrix;
	}

	static bool transformList ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;

		while ( comma( ref data ) ) { }
		if ( !transforms( ref data, out value ) ) {
			data = checkpoint;
			return false;
		}
		while ( comma( ref data ) ) { }
		return true;
	}

	static bool transforms ( ref ByteString data, out Matrix3<double> value ) {
		if ( !transform( ref data, out value ) )
			return false;

		var checkpoint = data;
		if ( !commaWsp( ref data ) )
			return true;
		while ( commaWsp( ref data ) ) { }

		if ( !transforms( ref data, out var next ) ) {
			data = checkpoint;
			return true;
		}

		value *= next;
		return true;
	}

	static bool transform ( ref ByteString data, out Matrix3<double> value ) {
		return matrix( ref data, out value )
			|| translate( ref data, out value )
			|| scale( ref data, out value )
			|| rotate( ref data, out value )
			|| skewX( ref data, out value )
			|| skewY( ref data, out value );
	}

	static bool matrix ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "matrix", 6, out var args ) || args.Length != 6 ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		args.Dispose();
		throw new NotImplementedException();
	}

	static bool translate ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "translate", 2, out var args ) || args.Length == 0 ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		//value = Matrix3<double>.CreateTranslation( args[0], -(args.Length == 2 ? args[1] : 0) );
		value = Matrix3<double>.Identity; // TODO what
		args.Dispose();
		return true;
	}

	static bool scale ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "scale", 2, out var args ) || args.Length == 0 ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		value = Matrix3<double>.CreateScale( args[0], args.Length == 2 ? args[1] : args[0] );
		args.Dispose();
		return true;
	}

	static bool rotate ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "rotate", 3, out var args ) || (args.Length != 1 && args.Length != 3) ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		if ( args.Length == 1 ) {
			value = Matrix3<double>.CreateRotation( args[0].Degrees() );
		}
		else {
			value = Matrix3<double>.CreateTranslation( -args[1], -args[2] ) 
				* Matrix3<double>.CreateRotation( args[0].Degrees() ) 
				* Matrix3<double>.CreateTranslation( args[1], args[2] );
		}
		args.Dispose();
		return true;
	}

	static bool skewX ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "skewX", 1, out var args ) || args.Length == 0 ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		value = Matrix3<double>.CreateShear( args[0], 0 );
		args.Dispose();
		return true;
	}

	static bool skewY ( ref ByteString data, out Matrix3<double> value ) {
		var checkpoint = data;
		if ( !function( ref data, "skewY", 1, out var args ) || args.Length != 1 ) {
			data = checkpoint;
			value = Matrix3<double>.Identity;
			return false;
		}

		value = Matrix3<double>.CreateShear( 0, args[0] );
		args.Dispose();
		return true;
	}

	static bool function ( ref ByteString data, string name, int maxArgs, out RentedArray<double> args ) {
		var checkpoint = data;

		args = default;
		if ( !literal( ref data, name ) )
			return false;

		while ( wsp( ref data ) ) { }
		if ( !single( ref data, '(' ) ) {
			data = checkpoint;
			return false;
		}
		
		while ( wsp( ref data ) ) { }
		args = new( maxArgs );
		args.Length = 0;
		for ( int i = 0; i < maxArgs; i++ ) {
			if ( Number.number( ref data, out var value ) ) {
				args.Length++;
				args[i] = value;

				if ( !commaWsp( ref data ) )
					break;
			}
			else {
				break;
			}
		}
		
		while ( wsp( ref data ) ) { }
		if ( !single( ref data, ')' ) ) {
			data = checkpoint;
			args.Dispose();
			return false;
		}

		return true;
	}

	static bool commaWsp ( ref ByteString data ) {
		if ( wsp( ref data ) ) {
			while ( wsp( ref data ) ) { }
			comma( ref data );
			while ( wsp( ref data ) ) { }
			return true;
		}

		if ( comma( ref data ) ) {
			while ( wsp( ref data ) ) { }
			return true;
		}

		return false;
	}

	static bool comma ( ref ByteString data ) {
		return single( ref data, ',' );
	}

	static bool wsp ( ref ByteString data ) {
		if ( data.Length == 0 )
			return false;

		if ( data[0] is ' ' or '\t' or '\n' or '\r' ) {
			data = data.Slice( 1 );
			return true;
		}
		return false;
	}

	static bool single ( ref ByteString data, char match ) {
		if ( data.Length == 0 )
			return false;

		if ( data[0] == match ) {
			data = data.Slice( 1 );
			return true;
		}
		return false;
	}

	static bool literal ( ref ByteString data, string match ) {
		if ( data.Length < match.Length )
			return false;

		for ( int i = 0; i < match.Length; i++ ) {
			if ( data[i] != match[i] )
				return false;
		}

		data = data.Slice( match.Length );
		return true;
	}
}
