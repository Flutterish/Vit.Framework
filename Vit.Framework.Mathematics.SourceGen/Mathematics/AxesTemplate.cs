namespace Vit.Framework.Mathematics.SourceGen.Mathematics;

public class AxesTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics";

	public override string GetTypeName ( int size ) {
		return $"Axes{size}";
	}

	protected override void GenerateOperators ( int size, SourceStringBuilder sb ) {
		GenerateComponentwiseOperator( size, sb, "+" );
		GenerateComponentwiseOperator( size, sb, "-" );

		var type = GetFullTypeName( size );
		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public static {type} operator - ( {type} axes ) {{" );
		using ( sb.Indent() ) {
			sb.Append( "return new( " );
			sb.AppendJoin( ", ", elements.Select( x => $"-axes.{AxisNames[x]}" ) );
			sb.AppendLine( " );" );
		}
		sb.AppendLine( "}" );

		GenerateComponentwiseScalarOperatorRight( size, sb, "*", leftName: "axes", rightName: "scale" );
		GenerateComponentwiseScalarOperatorLeft( size, sb, "*", leftName: "scale", rightName: "axes" );
		GenerateComponentwiseScalarOperatorRight( size, sb, "/", leftName: "axes", rightName: "divisor" );
	}
}
