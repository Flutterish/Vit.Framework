using Vit.Framework.Mathematics.SourceGen.Mathematics;

namespace Vit.Framework.Mathematics.SourceGen.Layout;

public class FlowSizeTemplate : SizeTemplate {
	protected override string Namespace => "Vit.Framework.Graphics.TwoD.Layout";

	public FlowSizeTemplate () {
		AxisNames = new[] { "Flow", "Cross" };
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics;" );
	}

	public override string GetTypeName ( int size ) {
		return $"FlowSize{size}";
	}
}
