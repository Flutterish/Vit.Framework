namespace Vit.Framework.Mathematics.SourceGen;

public class PointTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics";

	VectorTemplate vector = new() { Path = string.Empty };
	public override string GetTypeName ( int size ) {
		return $"Point{size}";
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics.LinearAlgebra;" );
	}

	protected override void GenerateMethods ( int size, SourceStringBuilder sb ) {
		var vectorType = $"{vector.GetTypeName( size )}<T>";
		var type = $"{GetTypeName( size )}<T>";
		var elements = Enumerable.Range( 0, size );

		sb.AppendLine();
		sb.AppendLine( $"public {vectorType} FromOrigin () {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{vector.AxisNames[x]} = {AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {vectorType} ToOrigin () {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{vector.AxisNames[x]} = -{AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {type} ScaleAboutOrigin ( T scale ) {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{AxisNames[x]} = {AxisNames[x]} * scale" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );

		sb.AppendLine();
		sb.AppendLine( $"public {type} ReflectAboutOrigin () {{" );
		using ( sb.Indent() ) {
			sb.AppendLine( $"return new() {{" );
			using ( sb.Indent() )
				sb.AppendLinePostJoin( ",", elements.Select( x => $"{AxisNames[x]} = -{AxisNames[x]}" ) );
			sb.AppendLine();
			sb.AppendLine( "};" );
		}
		sb.AppendLine( "}" );
	}

	protected override void GenerateOperators ( int size, SourceStringBuilder sb ) {
		GenerateComponentwiseOperator( size, sb, "+", right: vector, leftName: "point", rightName: "delta" );
		GenerateComponentwiseOperator( size, sb, "-", right: vector, leftName: "point", rightName: "delta" );

		GenerateComponentwiseOperator( size, sb, "-", result: vector, leftName: "to", rightName: "from" );
	}

	protected override void GenerateToString ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		sb.Append( "return $\"(" );
		sb.AppendJoin( ", ", elements.Select( x => $"{{{AxisNames[x]}}}" ) );
		sb.AppendLine( ")\";" );
	}
}
