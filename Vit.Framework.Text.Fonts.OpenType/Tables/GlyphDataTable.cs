using System.Diagnostics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

[ParsingDependsOn(typeof(MaximumProfileTable), typeof(IndexToLocationTable))]
public class GlyphDataTable : Table {
	[ParseWith(nameof(parse))]
	public GlyphData?[] Glyphs = null!;
	static GlyphData?[] parse ( MaximumProfileTable maxp, IndexToLocationTable loca, BinaryFileParser.Context context ) {
		var data = new GlyphData?[maxp.GlyphCount];
		var pos = context.Reader.Stream.Position;
		var offsets = loca.Offsets.GetEnumerator();
		if ( !offsets.MoveNext() )
			return data;

		var startOffset = offsets.Current;
		for ( int i = 0; i < data.Length; i++ ) {
			offsets.MoveNext();
			var endOffset = offsets.Current;
			var length = endOffset - startOffset;

			context.Reader.Stream.Position = context.Offset + startOffset;
			if ( length == 0 ) {
				data[i] = null;
			}
			else {
				data[i] = BinaryFileParser.Parse<GlyphData>( context );
			}

			startOffset = endOffset;
		}
		context.Reader.Stream.Position = pos;
		return data;
	}

	[TypeSelector(nameof(selectType))]
	public abstract class GlyphData {
		public short NumberOfContours;
		public short MinX;
		public short MinY;
		public short MaxX;
		public short MaxY;

		static Type? selectType ( short numberOfContours ) {
			return numberOfContours switch {
				< 0 => typeof( CompositeGlyphData ),
				_ => typeof( SimpleGlyphData )
			};
		}

		public abstract void CopyOutline ( Outline<double> outline );
	}

	public class SimpleGlyphData : GlyphData {
		[Size(nameof(NumberOfContours))]
		public ushort[] EndPointsOfContuours = null!;
		public ushort InstructionLength;
		[Size(nameof(InstructionLength))]
		public byte[] Instructions = null!;

		[ParseWith(nameof(parse))]
		public Point[] Points = null!;

		static Point[] parse ( ushort[] endPointsOfContuours, EndianCorrectingBinaryReader reader ) {
			var pointCount = endPointsOfContuours[^1] + 1;
			var points = new Point[pointCount];
			using var flags = new RentedArray<Flags>( pointCount );

			for ( int i = 0; i < pointCount; i++ ) {
				var flag = reader.Read<Flags>();
				flags[i] = flag;

				if ( flag.HasFlag( Flags.Repeat ) ) {
					var count = reader.Read<byte>();
					for ( int j = 0; j < count; j++ ) {
						flags[++i] = flag;
					}
				}
			}

			RentedArray<short> readCoords ( Flags positiveOrRepeatFlag, Flags isByteFlag ) {
				var coords = new RentedArray<short>( pointCount );
				for ( int i = 0; i < pointCount; i++ ) {
					var flag = flags[i];
					var posrep = flag.HasFlag( positiveOrRepeatFlag );

					if ( flag.HasFlag( isByteFlag ) ) {
						coords[i] = (short)( posrep ? reader.Read<byte>() : -reader.Read<byte>() );
					}
					else if ( posrep ) {
						coords[i] = 0;
					}
					else {
						coords[i] = reader.Read<short>();
					}
				}

				return coords;
			}

			using var xs = readCoords( Flags.XPositiveOrRepeat, Flags.XByte );
			using var ys = readCoords( Flags.YPositiveOrRepeat, Flags.YByte );

			for ( int i = 0; i < pointCount; i++ ) {
				points[i] = new() {
					X = xs[i],
					Y = ys[i],
					IsOnCurve = flags[i].HasFlag( Flags.OnCurve )
				};
			}

			return points;
		}

		public override void CopyOutline ( Outline<double> outline ) {
			if ( !Points.Any() )
				return;

			var i = 0;
			var point = new Point2<double>( 0, 0 );
			foreach ( var endIndex in EndPointsOfContuours ) {
				var first = Points[i];
				void translate ( Point next ) {
					point.X += next.X;
					point.Y += next.Y;
				}
				translate( first );

				Debug.Assert( first.IsOnCurve );
				var wasLastOnCurve = true;
				var control = new Point2<double>();
				var spline = new Spline2<double>( point );

				void addPoint ( Point2<double> next, bool onCurve ) {
					if ( wasLastOnCurve && onCurve ) {
						spline.AddLine( next );
					}
					else if ( wasLastOnCurve && !onCurve ) {
						control = next;
					}
					else if ( onCurve ) {
						spline.AddQuadraticBezier( control, next );
					}
					else {
						var ghost = control + (next - control) / 2;
						spline.AddQuadraticBezier( control, ghost );
						control = next;
					}

					wasLastOnCurve = onCurve;
				}

				for ( i++; i <= endIndex; i++ ) {
					var next = Points[i];
					translate( next );

					addPoint( point, next.IsOnCurve );
				}
				addPoint( spline.Points[0], onCurve: true );

				outline.Splines.Add( spline );
			}
		}

		[Flags]
		public enum Flags : byte {
			OnCurve = 1 << 0,
			XByte = 1 << 1,
			YByte = 1 << 2,
			Repeat = 1 << 3,
			XPositiveOrRepeat = 1 << 4,
			YPositiveOrRepeat = 1 << 5
		}

		public struct Point {
			public short X;
			public short Y;
			public bool IsOnCurve;

			public override string ToString () {
				return $"[{X}; {Y}] {(IsOnCurve ? "on curve" : "off curve")}";
			}
		}
	}

	public class CompositeGlyphData : GlyphData {
		public override void CopyOutline ( Outline<double> outline ) {
			//throw new NotImplementedException();
		}
	}
}
