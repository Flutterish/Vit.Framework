using Vit.Framework.Mathematics.SourceGen.Mathematics;
using Vit.Framework.Mathematics.SourceGen.Mathematics.GeometricAlgebra;

namespace Vit.Framework.Mathematics.SourceGen.Layout;

public class FlowVectorTemplate : VectorTemplate {
	protected override PointTemplate CreatePointTemplate () => new FlowPointTemplate() { Path = string.Empty };
	protected override BiVectorTemplate CreateBiVectorTemplate () => new FlowBiVectorTemplate { Path = string.Empty };

	protected override string Namespace => "Vit.Framework.TwoD.Layout";

	public FlowVectorTemplate () {
		AxisNames = new[] { "Flow", "Cross" };
	}

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics;" );
	}

	public override string GetTypeName ( int size ) {
		return $"FlowVector{size}";
	}
}
