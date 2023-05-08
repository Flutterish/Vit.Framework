namespace Vit.Framework.Mathematics.SourceGen;

public class SizeTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics";

	public SizeTemplate () {
		AxisNames = new[] { "Width", "Height", "Depth", "Anakata" };
	}

	public override string GetTypeName ( int size ) {
		return $"Size{size}";
	}

	protected override void GenerateToString ( int size, SourceStringBuilder sb ) {
		var elements = Enumerable.Range( 0, size );
		sb.Append( "return $\"" );
		sb.AppendJoin( "x", elements.Select( x => $"{{{AxisNames[x]}}}" ) );
		sb.AppendLine( "\";" );
	}
}
