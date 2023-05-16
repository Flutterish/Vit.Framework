using Vit.Framework.Mathematics.SourceGen.Mathematics.GeometricAlgebra;

namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class VectorTemplate : SpanLikeTemplate {
	protected virtual PointTemplate CreatePointTemplate () => new() { Path = string.Empty };
	PointTemplate? _point;
	PointTemplate point => _point ??= CreatePointTemplate();

	protected virtual BiVectorTemplate CreateBiVectorTemplate () => new() { Path = string.Empty };
	BiVectorTemplate? _biVector;
	BiVectorTemplate biVector => _biVector ??= CreateBiVectorTemplate();

	protected override string Namespace => "Vit.Framework.Mathematics";

	public override string GetTypeName ( int size ) {
		return $"Vector{size}";
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics.GeometricAlgebra;" );
	}

	protected override void GenerateProperties ( int size, SourceStringBuilder sb ) {
		var nonGenericType = GetTypeName( size );
		var type = $"{nonGenericType}<T>";
		var elements = Enumerable.Range( 0, size );

		sb.AppendLine();
		sb.Append( "public T LengthSquared => " );
		sb.AppendJoin( " + ", elements.Select( x => $"{AxisNames[x]} * {AxisNames[x]}" ) );
		sb.AppendLine( ";" );

		if ( size >= 2 )
			sb.AppendLine();
		for ( int a = 0; a < size - 1; a++ ) {
			for ( int b = a + 1; b < size; b++ ) {
				sb.Append( $"public {type} RotatedBy{AxisNames[a]}{AxisNames[b]} => new( " );
				sb.AppendJoin( ", ", elements.Select( x => x == a ? $"-{AxisNames[b]}" : x == b ? $"{AxisNames[a]}" : $"{AxisNames[x]}" ) );
				sb.AppendLine( " );" );
				sb.Append( $"public {type} RotatedBy{AxisNames[b]}{AxisNames[a]} => new( " );
				sb.AppendJoin( ", ", elements.Select( x => x == a ? $"{AxisNames[b]}" : x == b ? $"-{AxisNames[a]}" : $"{AxisNames[x]}" ) );
				sb.AppendLine( " );" );
			}
		}

		if ( size == 2 ) {
			sb.AppendLine();
			sb.AppendLine( $"public {type} Left => new( -{AxisNames[1]}, {AxisNames[0]} );" );
			sb.AppendLine( $"public {type} Right => new( {AxisNames[1]}, -{AxisNames[0]} );" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public Mathematics.LinearAlgebra.Generic.Vector<T> AsUnsized () => new( AsSpan() );" );
	}

	protected override void GenerateMethods ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		var type = GetFullTypeName( size );

		sb.AppendLine();
		sb.AppendLine( $"public T Dot ( {type} other )" );
		sb.AppendLine( $"\t=> Inner( this, other );" );
		sb.AppendLine( $"public static T Dot ( {type} left, {type} right )" );
		sb.AppendLine( $"\t=> Inner( left, right );" );
		sb.AppendLine( $"public T Inner ( {type} other )" );
		sb.AppendLine( $"\t=> Inner( this, other );" );
		sb.AppendLine( $"public static T Inner ( {type} left, {type} right ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return " );
			using var _ = sb.Indent();
			sb.AppendLinePreJoin( "+ ", elements.Select( i => $"left.{AxisNames[i]} * right.{AxisNames[i]}" ) );
			sb.AppendLine( ";" );
		}
		sb.AppendLine( "}" );

		if ( size >= 2 ) {
			var bivectorType = biVector.GetFullTypeName( size );
			var vectorType = GetFullTypeName( size );
			sb.AppendLine();
			var crossName = AxisNames.Contains( "Cross" ) ? "CrossProduct" : "Cross";
			sb.AppendLine( $"public {bivectorType} {crossName} ( {vectorType} other )" );
			sb.AppendLine( $"\t=> Outer( this, other );" );
			sb.AppendLine( $"public static {bivectorType} {crossName} ( {vectorType} left, {vectorType} right )" );
			sb.AppendLine( $"\t=> Outer( left, right );" );
			sb.AppendLine( $"public {bivectorType} Outer ( {vectorType} other )" );
			sb.AppendLine( $"\t=> Outer( this, other );" );
			sb.AppendLine( $"public static {bivectorType} Outer ( {vectorType} left, {vectorType} right ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					var result = BasisVectors.MakeVector( size, BasisVectors.ANames ).OuterProduct( BasisVectors.MakeVector( size, BasisVectors.BNames ) );
					foreach ( var i in result.Components ) {
						var scale = i.Scale;
						var basis = i.Bases;

						sb.AppendJoin( "", basis.Select( x => AxisNames[x.Name switch {
							"X" => 0,
							"Y" => 1,
							"Z" => 2,
							"Z" or _ => 3,
						}] ) );
						sb.Append( " = " );
						biVector.ScaleToString( scale, sb, multiline: false, aName: "left", bName: "right" );
						sb.AppendLine( "," );
					}
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public {point.GetFullTypeName( size )} FromOrigin () {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{point.AxisNames[x]} = {AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	protected override void GenerateOperators ( int size, SourceStringBuilder sb ) {
		GenerateComponentwiseOperator( size, sb, "+" );
		GenerateComponentwiseOperator( size, sb, "-" );

		var type = GetFullTypeName( size );
		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public static {type} operator - ( {type} vector ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return new( " );
			sb.AppendJoin( ", ", elements.Select( x => $"-vector.{AxisNames[x]}" ) );
			sb.AppendLine( " );" );
		}
		sb.AppendLine( "}" );

		GenerateComponentwiseScalarOperatorRight( size, sb, "*", leftName: "vector", rightName: "scale" );
		GenerateComponentwiseScalarOperatorLeft( size, sb, "*", leftName: "scale", rightName: "vector" );
		GenerateComponentwiseScalarOperatorRight( size, sb, "/", leftName: "vector", rightName: "divisor" );
	}

	protected override void GenerateToString ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		sb.Append( "return $\"[" );
		sb.AppendJoin( ", ", elements.Select( x => $"{{{AxisNames[x]}}}" ) );
		sb.AppendLine( "]\";" );
	}

	protected override void GenerateAfter ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		var type = GetFullTypeName( size );

		sb.AppendLine();
		sb.AppendLine( $"public static class Vector{size}Extensions {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"public static T GetLength<T> ( this {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return T.Sqrt( vector.LengthSquared );" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static {type} Normalized<T> ( this {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return vector / vector.GetLength();" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static void Normalize<T> ( this ref {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"var scale = T.MultiplicativeIdentity / vector.GetLength();" );
				foreach ( var i in elements ) {
					sb.AppendLine( $"vector.{AxisNames[i]} *= scale;" );
				}
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static T GetLengthFast<T> ( this {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return T.MultiplicativeIdentity / T.ReciprocalSqrtEstimate( vector.LengthSquared );" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static {type} NormalizedFast<T> ( this {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"return vector * T.ReciprocalSqrtEstimate( vector.LengthSquared );" );
			}
			sb.AppendLine( "}" );

			sb.AppendLine();
			sb.AppendLine( $"public static void NormalizeFast<T> ( this ref {type} vector ) where T : IFloatingPointIeee754<T> {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( $"var scale = T.ReciprocalSqrtEstimate( vector.LengthSquared );" );
				foreach ( var i in elements ) {
					sb.AppendLine( $"vector.{AxisNames[i]} *= scale;" );
				}
			}
			sb.AppendLine( "}" );

			if ( size == 2 ) {
				sb.AppendLine();
				sb.AppendLine( $"public static Radians<T> GetAngle<T> ( this {type} vector ) where T : IFloatingPointIeee754<T> {{" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"return T.Atan2( vector.{AxisNames[1]}, vector.{AxisNames[0]} ).Radians();" );
				}
				sb.AppendLine( "}" );
			}
		}
		sb.AppendLine( "}" );
	}
}
