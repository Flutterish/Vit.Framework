using Vit.Framework.Mathematics.SourceGen.Mathematics;
using Vit.Framework.Mathematics.SourceGen.Mathematics.GeometricAlgebra;

namespace Vit.Framework.Mathematics.SourceGen.Layout;

public class FlowBiVectorTemplate : BiVectorTemplate {
	protected override VectorTemplate CreateVectorTemplate () => new FlowVectorTemplate() { Path = string.Empty };
	protected override string Namespace => "Vit.Framework.TwoD.Layout";

	protected override void GenerateUsings ( int size, SourceStringBuilder sb ) {
		base.GenerateUsings( size, sb );
		sb.AppendLine( "using Vit.Framework.Mathematics;" );
	}

	public override string GetTypeName ( int size ) {
		return $"FlowBiVector{size}";
	}
}
