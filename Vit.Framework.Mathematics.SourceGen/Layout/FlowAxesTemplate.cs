namespace Vit.Framework.Mathematics.SourceGen.Layout;

public class FlowAxesTemplate : AxesTemplate {
	protected override string Namespace => "Vit.Framework.Graphics.TwoD.Layout";

	public FlowAxesTemplate () {
		AxisNames = new[] { "Flow", "Cross" };
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics;" );
	}

	public override string GetTypeName ( int size ) {
		return $"FlowAxes{size}";
	}
}
