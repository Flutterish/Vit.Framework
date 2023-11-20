using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Formats.Tar;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using Vit.Framework.Graphics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.Text.Fonts.OpenType.Svg;

public class Path : SvgElement {
	ColorSRgba<byte>? fillValue;
	List<Spline2<double>> data = new();
	FillRule fillRule;
	FillRule clipRule;
	double opacity;
	public override void Open ( ref SvgOutline.Context context ) {
		base.Open( ref context );
		fillValue = ColorSRgba.Black.ToByte();
		fillRule = FillRule.EvenOdd;
		clipRule = FillRule.EvenOdd;
		opacity = 1;
	}

	HeapByteString fill = "fill";
	HeapByteString fillRuleAttribute = "fill-rule";
	HeapByteString clipRuleAttribute = "clip-rule";
	HeapByteString opacityAttribute = "opacity";
	HeapByteString d = "d";

	HeapByteString evenodd = "evenodd";
	HeapByteString nonzero = "nonzero";
	public override bool SetAttribute ( ref SvgOutline.Context context, ByteString name, ByteString unescapedValue ) {
		if ( name == fill ) {
			fillValue = Color.Parse( unescapedValue );
		}
		else if ( name == fillRuleAttribute ) {
			if ( unescapedValue == evenodd ) {
				fillRule = FillRule.EvenOdd;
			}
			else if ( unescapedValue == nonzero ) {
				fillRule = FillRule.NonZero;
			}
			else {
				throw new InvalidDataException();
			}
		}
		else if ( name == clipRuleAttribute ) {
			if ( unescapedValue == evenodd ) {
				clipRule = FillRule.EvenOdd;
			}
			else if ( unescapedValue == nonzero ) {
				clipRule = FillRule.NonZero;
			}
			else {
				throw new InvalidDataException();
			}
		}
		else if ( name == opacityAttribute ) {
			opacity = Number.Parse( unescapedValue );
		}
		else if ( name == d ) {
			var data = unescapedValue;

			static void skipWhitespace ( ref ByteString data ) {
				while ( data.Length > 0 && (char.IsWhiteSpace( data[0] ) || data[0] == ',') ) {
					data = data[1..];
				}
			}

			static CommandType commandType ( char value ) {
				return value switch {
					'm' => CommandType.MoveBy,
					'M' => CommandType.MoveTo,
					'l' => CommandType.LineBy,
					'L' => CommandType.LineTo,
					'v' => CommandType.VerticalLineBy,
					'V' => CommandType.VerticalLineTo,
					'h' => CommandType.HorizontalLineBy,
					'H' => CommandType.HorizontalLineTo,
					'c' => CommandType.CurveBy,
					'C' => CommandType.CurveTo,
					's' => CommandType.SmoothCurveBy,
					'S' => CommandType.SmoothCurveTo,
					'q' => CommandType.QuadraticCurveBy,
					'Q' => CommandType.QuadraticCurveTo,
					't' => CommandType.SmoothQuadraticCurveBy,
					'T' => CommandType.SmoothQuadraticCurveTo,
					'a' => CommandType.ArcBy,
					'A' => CommandType.ArcTo,
					'z' or 'Z' => CommandType.ClosePath,
					_ => CommandType.Unknown
				};
			}

			static bool nextCommand ( ref ByteString data, out CommandType type ) {
				if ( data.Length == 0 ) {
					type = CommandType.Unknown;
					return false;
				}

				type = commandType( data[0] );
				if ( type == CommandType.Unknown )
					return false;

				data = data[1..];
				return true;
			}

			static double parameter ( ref ByteString data ) {
				skipWhitespace( ref data );
				return Number.Consume( ref data );
			}

			static bool nextIsNumber ( ByteString data ) {
				return data.Length > 0 && commandType( data[0] ) == CommandType.Unknown;
			}

			static void processCommand ( ref ByteString data, ref Spline2<double>? subpath, List<Spline2<double>> splines, CommandType command ) {
				if ( command is CommandType.MoveTo or CommandType.MoveBy ) {
					var value = new Point2<double>( parameter( ref data ), parameter( ref data ) );

					if ( command is CommandType.MoveBy ) {
						splines.Add( subpath = (splines.Count == 0 ? new( value ) : new( splines[^1].Points[^1] + value.FromOrigin() )) );
					}
					else {
						splines.Add( subpath = new( value ) );
					}

					skipWhitespace( ref data );
					if ( nextIsNumber( data ) ) {
						processCommand( ref data, ref subpath, splines, command == CommandType.MoveTo ? CommandType.LineTo : CommandType.LineBy );
					}

					return;
				}
				
				if ( subpath == null ) {
					splines.Add( subpath = new( splines[^1].Points[^1] ) );
				}

				if ( command is CommandType.LineTo or CommandType.LineBy ) {
					do {
						var value = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						if ( command is not CommandType.LineTo ) {
							var current = subpath.Points[^1].FromOrigin();
							value += current;
						}

						subpath.AddLine( value );
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.HorizontalLineTo or CommandType.HorizontalLineBy ) {
					do {
						var value = parameter( ref data );
						var current = subpath.Points[^1];

						if ( command is CommandType.HorizontalLineTo ) {
							subpath.AddLine( current with { X = value } );
						}
						else {
							subpath.AddLine( current with { X = current.X + value } );
						}

						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.VerticalLineTo or CommandType.VerticalLineBy ) {
					do {
						var value = parameter( ref data );
						var last = subpath.Points[^1];

						if ( command is CommandType.VerticalLineTo ) {
							subpath.AddLine( last with { Y = value } );
						}
						else {
							subpath.AddLine( last with { Y = last.Y + value } );
						}

						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.CurveTo or CommandType.CurveBy ) {
					do {
						var cp1 = new Point2<double>( parameter( ref data ), parameter( ref data ) );
						var cp2 = new Point2<double>( parameter( ref data ), parameter( ref data ) );
						var end = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						if ( command is not CommandType.CurveTo ) {
							var current = subpath.Points[^1].FromOrigin();
							cp1 += current;
							cp2 += current;
							end += current;
						}

						subpath.AddCubicBezier( cp1, cp2, end );
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.SmoothCurveTo or CommandType.SmoothCurveBy ) {
					do {
						var cp2 = new Point2<double>( parameter( ref data ), parameter( ref data ) );
						var end = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						var current = subpath.Points[^1];
						if ( command is not CommandType.SmoothCurveTo ) {
							cp2 += current.FromOrigin();
							end += current.FromOrigin();
						}

						Point2<double> cp1;
						if ( subpath.Curves.Count != 0 && subpath.Curves[^1] is CurveType.BezierCubic ) {
							cp1 = current + (current - subpath.Points[^2]);
						}
						else {
							cp1 = current;
						}

						subpath.AddCubicBezier( cp1, cp2, end );
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.QuadraticCurveTo or CommandType.QuadraticCurveBy ) {
					do {
						var cp = new Point2<double>( parameter( ref data ), parameter( ref data ) );
						var end = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						if ( command is not CommandType.QuadraticCurveTo ) {
							var current = subpath.Points[^1].FromOrigin();
							cp += current;
							end += current;
						}

						subpath.AddQuadraticBezier( cp, end );
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.SmoothQuadraticCurveTo or CommandType.SmoothQuadraticCurveBy ) {
					do {
						var end = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						var current = subpath.Points[^1];
						if ( command is not CommandType.SmoothQuadraticCurveTo ) {
							end += current.FromOrigin();
						}

						Point2<double> cp;
						if ( subpath.Curves.Count != 0 && subpath.Curves[^1] is CurveType.BezierQuadratic ) {
							cp = current + (current - subpath.Points[^2]);
						}
						else {
							cp = current;
						}

						subpath.AddQuadraticBezier( cp, end );
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.ArcTo or CommandType.ArcBy ) {
					do {
						var radius = new Axes2<double>( parameter( ref data ), parameter( ref data ) );
						var xAxisRotation = parameter( ref data );
						var largeArcFlag = parameter( ref data );
						var sweepFlag = parameter( ref data );
						var end = new Point2<double>( parameter( ref data ), parameter( ref data ) );

						if ( command is not CommandType.ArcTo ) {
							var current = subpath.Points[^1].FromOrigin();
							end += current;
						}

						subpath.AddLine( end ); // TODO this should be an ellipse
						skipWhitespace( ref data );
					}
					while ( nextIsNumber( data ) );
				}
				else if ( command is CommandType.ClosePath ) {
					subpath.AddLine( subpath.Points[0] );
					subpath = null;
					skipWhitespace( ref data );
				}
			}

			skipWhitespace( ref data );
			Spline2<double>? subpath = null;
			while ( data.Length > 0 ) {
				if ( !nextCommand( ref data, out var command ) )
					throw new InvalidDataException();

				processCommand( ref data, ref subpath, this.data, command );
			}
		}
		else {
			throw new NotImplementedException();
		}

		return true;
	}

	enum CommandType {
		Unknown,
		MoveTo, MoveBy,
		LineTo, LineBy,
		HorizontalLineTo, HorizontalLineBy,
		VerticalLineTo, VerticalLineBy,
		CurveTo, CurveBy,
		SmoothCurveTo, SmoothCurveBy,
		QuadraticCurveTo, QuadraticCurveBy,
		SmoothQuadraticCurveTo, SmoothQuadraticCurveBy,
		ArcTo, ArcBy,
		ClosePath
	}

	public override void Close ( ref SvgOutline.Context context ) {
		base.Close( ref context );
		foreach ( var spline in data ) {
			for ( int i = 0; i < spline.Points.Count; i++ ) {
				spline.Points[i] = context.Matrix.Apply( spline.Points[i] );
			}
		}
		context.Outline.Elements.Add( new() {
			Fill = fillValue,
			Splines = data.ToArray(),
			FillRule = fillRule
		} );
		data.Clear();
	}
}
