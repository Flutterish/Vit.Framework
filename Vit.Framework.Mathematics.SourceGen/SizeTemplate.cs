namespace Vit.Framework.Mathematics.SourceGen;

public class SizeTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics";

	public SizeTemplate () {
		AxisNames = new[] { "Width", "Height", "Depth", "Anakata" };
	}

	public override string GetTypeName ( int size ) {
		return $"Size{size}";
	}

	protected override void GenerateMethods ( int size, SourceStringBuilder sb ) {
		base.GenerateMethods( size, sb );

		var elements = Enumerable.Range( 0, size );
		sb.AppendLine();
		sb.AppendLine( $"public {GetFullTypeName(size)} Contain ( {GetFullTypeName(size)} other ) => new() {{" );
		using ( sb.Indent() ) {
			foreach ( var i in elements ) {
				sb.AppendLine( $"{AxisNames[i]} = T.Max( {AxisNames[i]}, other.{AxisNames[i]} )," );
			}
		}
		sb.AppendLine( "};" );
	}

	protected override void GenerateOperators ( int size, SourceStringBuilder sb ) {
		GenerateComponentwiseScalarOperatorRight( size, sb, "*" );
		GenerateComponentwiseScalarOperatorLeft( size, sb, "*" );
		GenerateComponentwiseScalarOperatorRight( size, sb, "/" );
	}

	protected override void GenerateToString ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		sb.Append( "return $\"" );
		sb.AppendJoin( "x", elements.Select( x => $"{{{AxisNames[x]}}}" ) );
		sb.AppendLine( "\";" );
	}
}
