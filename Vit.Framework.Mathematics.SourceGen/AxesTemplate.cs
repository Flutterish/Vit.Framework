﻿namespace Vit.Framework.Mathematics.SourceGen;

public class AxesTemplate : SpanLikeTemplate {
	protected override string Namespace => "Vit.Framework.Mathematics";

	public override string GetTypeName ( int size ) {
		return $"Axes{size}";
	}
}
