using System.Diagnostics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Adobe;

public class CharStringInterpreter {
	Outline<double> outline;
	Index<CharString> globalSubrs;
	int globalBias;
	Index<CharString> localSubrs;
	int localBias;
	private CharStringInterpreter ( Outline<double> outline, Index<CharString> globalSubrs, Index<CharString> localSubrs ) {
		this.outline = outline;
		this.globalSubrs = globalSubrs;
		this.localSubrs = localSubrs;

		globalBias = getBias( globalSubrs.Count );
		localBias = getBias( localSubrs.Count );
	}

	int getBias ( int count ) {
		if ( count < 1240 )
			return 107;
		if ( count < 33900 )
			return 1131;
		return 32768;
	}

	public static void Load ( CharString charString, Glyph glyph, Index<CharString> globalSubrs, Index<CharString> localSubrs ) {
		var interpreter = new CharStringInterpreter( glyph.Outline, globalSubrs, localSubrs );
		interpreter.Execute( charString );
	}

	public Point2<double> Position;
	public Stack<double> ArgumentStack = new();
	public double? Width;
	public bool IsWidthPending = true;
	public string? LastHintType;
	public bool AreHintsSet;
	public Spline2<double>? Spline;
	public void Execute ( CharString charString ) {
		var data = charString.Data;
		execute( ref data );
	}

	public List<(double from, double to)> HorizontalHints = new();
	public List<(double from, double to)> VerticalHints = new();
	public List<string> Explain = new();
	public List<string> SVG = new(); // TODO remove - debug only
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
			outline.Splines.Add( Spline );
		}
		Spline = null;
	}

	public void AddCubicBezier ( double dxa, double dya, double dxb, double dyb, double dxc, double dyc, bool flip = false ) {
		var a = Position;
		var b = a + ( flip ? new Vector2<double>( dya, dxa ) : new Vector2<double>( dxa, dya ) );
		var c = b + new Vector2<double>( dxb, dyb );
		var d = c + ( flip ? new Vector2<double>( dyc, dxc ) : new Vector2<double>( dxc, dyc ) );
		Position = d;
		SVG.Add( $"Bezier curve from {a} through {b} and {c} to {d}" );

		Spline!.AddCubicBezier( b, c, d );
	}

	public void AddLine ( double dx, double dy, bool flip = false ) {
		var a = Position;
		var b = a + ( flip ? new Vector2<double>( dy, dx ) : new Vector2<double>( dx, dy ) );
		Position = b;
		SVG.Add( $"Line from {a} to {b}" );

		Spline!.AddLine( b );
	}

	void execute ( ref BinaryArrayView<byte> data ) {
		while ( data.Length != 0 ) {
			if ( tryConsumeNumber( ref data, out var number ) ) {
				Explain.Add( $"Push `{number}`" );
				ArgumentStack.Push( number );
			}
			else {
				consumeOperator( ref data );
			}
		}
	}

	bool tryConsumeNumber ( ref BinaryArrayView<byte> raw, out double value ) {
		if ( raw[0] is 28 or >= 32 ) {
			value = consumeNumber( ref raw );
			return true;
		}

		value = double.NaN;
		return false;
	}

	double consumeNumber ( ref BinaryArrayView<byte> raw ) {
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
			using var value = data[1..5].GetRented();
			return (double)BitConverter.ToInt32( value ) / ( 1 << 16 );
		}
		if ( data[0] is 28 ) {
			raw = raw[3..];
			using var value = data[1..3].GetRented();
			return BitConverter.ToInt16( value );
		}

		return double.NaN;
	}

	bool tryConsumeOperator ( ref BinaryArrayView<byte> raw ) {
		if ( raw[0] is not 28 and <= 31 ) {
			consumeOperator( ref raw );
			return true;
		}

		return false;
	}

	void consumeOperator ( ref BinaryArrayView<byte> raw ) {
		var b0 = raw[0];
		raw = raw[1..];

		if ( b0 is 14 ) { // end char
			pushWidth();
			Explain.Add( "End character" );
			Finish();
		}
		else if ( b0 is 1 or 3 or 18 or 23 ) { // hints
			Debug.Assert( !AreHintsSet );
			pushHints( b0 is 1 or 18 ? "horizontal" : "vertical", @implicit: false );
		}
		else if ( b0 is 19 or 20 ) { // hint masks
			if ( !AreHintsSet )
				pushHints( LastHintType == "horizontal" ? "vertical" : "horizontal", @implicit: true );

			pushWidth();
			var count = VerticalHints.Count + HorizontalHints.Count;
			var size = ( count + 7 ) / 8;
			using var mask = raw[..size].GetRented();
			raw = raw[size..];
			Explain.Add( $"Set hint {( b0 == 20 ? "counter" : "" )}mask ({count} hint(s)) to {Convert.ToHexString( mask )}" );
			AreHintsSet = true;
		}
		else if ( b0 is 21 or 22 or 4 ) { // move to
			var args = getArgs( b0 is 21 ? 2 : 1 );
			var dx = b0 is 21 or 22 ? args[0] : 0;
			var dy = b0 is 4 ? args[0] : b0 is 21 ? args[1] : 0;
			pushWidth();
			Explain.Add( $"Move to" );
			Begin( dx, dy );
		}
		else if ( b0 is 5 ) { // lines
			var count = ArgumentStack.Count / 2;
			Debug.Assert( count >= 1 );
			var vargs = getArgs( count * 2 );
			Explain.Add( $"Line to ({count} time(s))" );

			for ( int i = 0; i < count; i++ ) {
				var args = vargs.AsSpan().Slice( i * 2, 2 );
				AddLine( args[0], args[1] );
			}
		}
		else if ( b0 is 6 or 7 ) {
			var count = ArgumentStack.Count;
			Debug.Assert( count >= 1 );
			var vargs = getArgs( count );
			Explain.Add( $"Line to [{b0}] ({count} time(s))" );

			var flip = b0 is 7;
			for ( int i = 0; i < count; i++ ) {
				AddLine( vargs[i], 0, flip );
				flip = !flip;
			}
		}
		else if ( b0 is 8 ) { // bezier curves
			var count = ArgumentStack.Count / 6;
			Debug.Assert( count >= 1 );
			var vargs = getArgs( count * 6 );
			Explain.Add( $"Bezier curve ({count} time(s))" );

			for ( int i = 0; i < count; i++ ) {
				var args = vargs.AsSpan().Slice( i * 6, 6 );
				AddCubicBezier(
					args[0], args[1],
					args[2], args[3],
					args[4], args[5]
				);
			}
		}
		else if ( b0 is 26 or 27 ) {
			var count = ArgumentStack.Count / 4;
			var parity = ArgumentStack.Count % 4;
			Debug.Assert( count >= 1 && parity is 0 or 1 );
			var vargs = getArgs( parity + count * 4 );
			Explain.Add( $"Bezier curve [{b0}] ({count} time(s))" );

			for ( int i = 0; i < count; i++ ) {
				var args = vargs.AsSpan().Slice( parity + i * 4, 4 );
				var dxa = i == 0 && parity == 1 ? vargs[0] : 0;
				AddCubicBezier(
					dxa, args[0],
					args[1], args[2],
					0, args[3],
					flip: b0 is 27
				);
			}
		}
		else if ( b0 is 30 or 31 ) {
			var parity = ArgumentStack.Count % 8;
			var count = ArgumentStack.Count / 8;
			Debug.Assert( ( parity is 0 or 1 && count >= 1 ) || ( parity is 4 or 5 ) );
			var vargs = getArgs( parity + count * 8 );
			Explain.Add( $"Bezier curve [{b0}] ({( parity < 2 ? count : ( count + 1 ) )} time(s))" );

			bool flip = b0 == 31;
			int offset = 0;

			if ( parity >= 2 ) {
				var dy = count == 0 && parity == 5 ? vargs[^1] : 0;
				AddCubicBezier(
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
				AddCubicBezier(
					0, args[0],
					args[1], args[2],
					args[3], 0,
					flip
				);

				var dx = i == ( count - 1 ) && parity == ( 1 + offset ) ? vargs[^1] : 0;
				AddCubicBezier(
					args[4], 0,
					args[5], args[6],
					dx, args[7],
					flip
				);
			}
		}
		else if ( b0 is 24 ) { // combinations
			var count = ( ArgumentStack.Count - 2 ) / 6;
			Debug.Assert( count >= 1 && ArgumentStack.Count % 6 == 2 );
			var vargs = getArgs( count * 6 + 2 );
			Explain.Add( $"Bezier curve ({count} time(s)) + line to" );

			for ( int i = 0; i < count; i++ ) {
				var args = vargs.AsSpan().Slice( i * 6, 6 );
				AddCubicBezier(
					args[0], args[1],
					args[2], args[3],
					args[4], args[5]
				);
			}
			AddLine( vargs[^2], vargs[^1] );
		}
		else if ( b0 is 25 ) {
			var count = ( ArgumentStack.Count - 6 ) / 2;
			Debug.Assert( ArgumentStack.Count >= 8 && ArgumentStack.Count % 2 == 0 );
			var vargs = getArgs( count * 2 + 6 );
			Explain.Add( $"Line to ({count} time(s)) + bezier curve" );

			for ( int i = 0; i < count; i++ ) {
				var args = vargs.AsSpan().Slice( i * 2, 2 );
				AddLine( args[0], args[1] );
			}
			AddCubicBezier(
				vargs[^6], vargs[^5],
				vargs[^4], vargs[^3],
				vargs[^2], vargs[^1]
			);
		}
		else if ( b0 is 10 ) { // call local subroutine
			var arg = (int)getArgs( 1 )[0] + localBias;
			Explain.Add( $"(start local subroutine {arg})" );
			BinaryArrayView<byte> subr = localSubrs[arg].Data;
			execute( ref subr );
			Explain.Add( $"(end local subroutine {arg})" );
		}
		else if ( b0 is 29 ) { // call global subroutine
			var arg = (int)getArgs( 1 )[0] + globalBias;
			Explain.Add( $"(start global subroutine {arg})" );
			BinaryArrayView<byte> subr = globalSubrs[arg].Data;
			execute( ref subr );
			Explain.Add( $"(end global subroutine {arg})" );
		}
		else if ( b0 is 11 ) { // return
			Explain.Add( $"(return)" );
		}
		else {
			throw new NotImplementedException( "oops" );
		}
	}

	double[] getArgs ( int count ) {
		var args = new double[count];
		for ( int i = 0; i < count; i++ ) {
			args[count - 1 - i] = ArgumentStack.Pop();
		}

		return args;
	}

	void pushHints ( string type, bool @implicit ) {
		var count = ArgumentStack.Count / 2;
		getArgs( count * 2 );
		pushWidth();

		Explain.Add( $"{( @implicit ? "Implicitly add" : "Add" )} {count} {type} hint(s)" );
		var list = type == "horizontal" ? HorizontalHints : VerticalHints;
		for ( int i = 0; i < count; i++ ) {
			list.Add( (0, 0) ); // TODO
		}
		LastHintType = type;

	}

	void pushWidth () {
		if ( !IsWidthPending )
			return;

		IsWidthPending = false;
		if ( ArgumentStack.TryPop( out var width ) ) {
			Explain.Add( $"Width = {width}" );
			Width = width;
		}
	}
}
