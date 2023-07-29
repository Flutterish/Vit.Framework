using Vit.Framework.Mathematics.SourceGen.Mathematics;

namespace Vit.Framework.Mathematics.SourceGen.Layout;

public class FlowPointTemplate : PointTemplate {
	protected override VectorTemplate CreateVectorTemplate () => new FlowVectorTemplate() { Path = string.Empty };
	protected override string Namespace => "Vit.Framework.TwoD.Layout";

	public FlowPointTemplate () {
		AxisNames = new[] { "Flow", "Cross" };
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics;" );
	}

	public override string GetTypeName ( int size ) {
		return $"FlowPoint{size}";
	}
}
