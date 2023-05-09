namespace Vit.Framework.Mathematics.SourceGen;

public class VectorTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics.LinearAlgebra";

	public override string GetTypeName ( int size ) {
		return $"Vector{size}";
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
				sb.Append( $"public {type} LeftIn{AxisNames[a]}{AxisNames[b]} => new( " );
				sb.AppendJoin( ", ", elements.Select( x => x == a ? $"-{AxisNames[b]}" : x == b ? $"{AxisNames[a]}" : $"{AxisNames[x]}" ) );
				sb.AppendLine( " );" );
				sb.Append( $"public {type} RightIn{AxisNames[a]}{AxisNames[b]} => new( " );
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
		sb.AppendLine( $"public Generic.Vector<T> AsUnsized () => new( AsSpan() );" );
	}

	protected override void GenerateMethods ( int size, SourceStringBuilder sb ) {
		var point = new PointTemplate { Path = string.Empty };
		var elements = Enumerable.Range( 0, size );
		var type = GetFullTypeName( size );

		sb.AppendLine();
		sb.AppendLine( $"public T Dot ( {type} other )" );
		sb.AppendLine( $"\t=> Dot( this, other );" );
		sb.AppendLine( $"public static T Dot ( {type} left, {type} right ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return " );
			using var _ = sb.Indent();
			sb.AppendLinePreJoin( "+ ", elements.Select( i => $"left.{AxisNames[i]} * right.{AxisNames[i]}" ) );
			sb.AppendLine( ";" );
		}
		sb.AppendLine( "}" );

		if ( size == 3 ) {
			sb.AppendLine();
			sb.AppendLine( $"public {type} Cross ( {type} other )" );
			sb.AppendLine( $"\t=> Cross( this, other );" );
			sb.AppendLine( $"public static {type} Cross ( {type} left, {type} right ) {{" );
			using ( sb.Indent() ) {
				sb.AppendLine( "return new() {" );
				using ( sb.Indent() ) {
					sb.AppendLine( $"{AxisNames[0]} = left.{AxisNames[1]} * right.{AxisNames[2]} - left.{AxisNames[2]} * right.{AxisNames[1]}," );
					sb.AppendLine( $"{AxisNames[1]} = left.{AxisNames[2]} * right.{AxisNames[0]} - left.{AxisNames[0]} * right.{AxisNames[2]}," );
					sb.AppendLine( $"{AxisNames[2]} = left.{AxisNames[0]} * right.{AxisNames[1]} - left.{AxisNames[1]} * right.{AxisNames[0]}" );
				}
				sb.AppendLine( "};" );
			}
			sb.AppendLine( "}" );
		}

		sb.AppendLine();
		sb.AppendLine( $"public {point.GetFullTypeName(size)} FromOrigin () {{" );
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
