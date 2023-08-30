using System.Diagnostics;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.Curves;
using Vit.Framework.Memory;
using Vit.Framework.Parsing;
using Vit.Framework.Parsing.Binary;
using Vit.Framework.Text.Outlines;

namespace Vit.Framework.Text.Fonts.OpenType.Tables;

public class GlyphDataTable : Table {
	BinaryViewContext context;

	public GlyphData? GetGlyph ( GlyphId id ) {
		var loca = context.ResolveDependency<IndexToLocationTable>();

		var index = (int)id.Value;
		var startOffset = loca[index];
		var endOffset = loca[index + 1];
		var length = endOffset - startOffset;
		if ( length == 0 )
			return null;

		return BinaryView<GlyphData>.Parse( context with { Offset = context.OffsetAnchor + startOffset } );
	}

	public GlyphDataHeader? GetHeader ( GlyphId id ) {
		var loca = context.ResolveDependency<IndexToLocationTable>();

		var index = (int)id.Value;
		var startOffset = loca[index];
		var endOffset = loca[index + 1];
		var length = endOffset - startOffset;
		if ( length == 0 )
			return null;

		return BinaryView<GlyphDataHeader>.Parse( context with { Offset = context.OffsetAnchor + startOffset } );
	}

	public struct GlyphDataHeader {
		public short NumberOfContours;
		public short MinX;
		public short MinY;
		public short MaxX;
		public short MaxY;
	}

	[TypeSelector( nameof( selectType ) )]
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

		public abstract void CopyOutline ( SplineOutline outline, GlyphDataTable glyphs );
	}

	public class SimpleGlyphData : GlyphData {
		[Size( nameof( NumberOfContours ) )]
		public ushort[] EndPointsOfContuours = null!;
		public ushort InstructionLength;
		[Size( nameof( InstructionLength ) )]
		public byte[] Instructions = null!;

		[ParseWith( nameof( parse ) )]
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

		public override void CopyOutline ( SplineOutline outline, GlyphDataTable glyphs ) {
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

				// TODO Debug.Assert( first.IsOnCurve );
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
						var ghost = control + ( next - control ) / 2;
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
				return $"[{X}; {Y}] {( IsOnCurve ? "on curve" : "off curve" )}";
			}
		}
	}

	public class CompositeGlyphData : GlyphData {
		[ParseWith( nameof( parse ) )]
		public ComponentGlyphData[] Components = null!;

		static ComponentGlyphData[] parse ( EndianCorrectingBinaryReader reader ) {
			List<ComponentGlyphData> data = new();
			Flags flags;
			do {
				flags = reader.Read<Flags>();
				var index = reader.Read<ushort>();
				ushort arg1;
				ushort arg2;
				if ( flags.HasFlag( Flags.ArgsAre16Bit ) ) {
					arg1 = reader.Read<ushort>();
					arg2 = reader.Read<ushort>();
				}
				else {
					arg1 = reader.Read<byte>();
					arg2 = reader.Read<byte>();
				}

				Fixed2_14 scaleX;
				Fixed2_14 scaleY;
				Fixed2_14 shearX;
				Fixed2_14 shearY;
				if ( flags.HasFlag( Flags.HasScale ) ) {
					scaleX = scaleY = reader.Read<Fixed2_14>();
					shearX = shearY = Fixed2_14.Zero;
				}
				else if ( flags.HasFlag( Flags.SeparateScales ) ) {
					scaleX = reader.Read<Fixed2_14>();
					scaleY = reader.Read<Fixed2_14>();
					shearX = shearY = Fixed2_14.Zero;
				}
				else if ( flags.HasFlag( Flags.HasTransfomrationMatrix ) ) {
					Debug.Fail( "figure the todo out" );
					scaleX = reader.Read<Fixed2_14>();
					shearX = reader.Read<Fixed2_14>(); // TODO idk the format of the matrix they give
					shearY = reader.Read<Fixed2_14>();
					scaleY = reader.Read<Fixed2_14>();
				}
				else {
					scaleX = scaleY = Fixed2_14.One;
					shearX = shearY = Fixed2_14.Zero;
				}

				data.Add( new() {
					Flags = flags,
					GlyphIndex = index,
					Arg1 = arg1,
					Arg2 = arg2,

					ScaleX = scaleX,
					ScaleY = scaleY,
					ShearX = shearX,
					ShearY = shearY
				} );
			}
			while ( flags.HasFlag( Flags.MoreComponents ) );

			Debug.Assert( !flags.HasFlag( Flags.HasInstructions ) );

			return data.ToArray();
		}

		public override void CopyOutline ( SplineOutline outline, GlyphDataTable glyphs ) {
			foreach ( var i in Components ) {
				Debug.Assert( i.Flags.HasFlag( Flags.ArgsAreXyValues ) );
				//Debug.Assert( i.Flags.HasFlag( Flags.ArgsAre16Bit ) || (i.Arg1 == 0 && i.Arg2 == 0) );
				// TODO round to XY grid
				Debug.Assert( i.ScaleX == Fixed2_14.One );
				Debug.Assert( i.ScaleY == Fixed2_14.One );
				Debug.Assert( i.ShearX == Fixed2_14.Zero );
				Debug.Assert( i.ShearY == Fixed2_14.Zero );

				//var dx = new Fixed2_14 { Data = i.Arg1.BitCast<ushort, short>() };
				//var dy = new Fixed2_14 { Data = i.Arg2.BitCast<ushort, short>() };

				var index = i.GlyphIndex;
				var glyph = glyphs.GetGlyph( index )!;
				Debug.Assert( glyph is SimpleGlyphData );
				glyph.CopyOutline( outline, glyphs );
			}
		}

		[Flags]
		public enum Flags : ushort {
			ArgsAre16Bit = 0x1,
			ArgsAreXyValues = 0x2,
			RoundXyToGrid = 0x4,
			HasScale = 0x8,
			MoreComponents = 0x20,
			SeparateScales = 0x40,
			HasTransfomrationMatrix = 0x80,
			HasInstructions = 0x100,
			UseMyMetrics = 0x200,
			ComponentsOverlap = 0x400,
			ScaledComponentOffset = 0x800,
			UnscaledComponentOffset = 0x1000
		}

		public struct ComponentGlyphData {
			public Flags Flags;
			public ushort GlyphIndex;

			public ushort Arg1;
			public ushort Arg2;

			public Fixed2_14 ScaleX;
			public Fixed2_14 ScaleY;
			public Fixed2_14 ShearX;
			public Fixed2_14 ShearY;
		}
	}
}
