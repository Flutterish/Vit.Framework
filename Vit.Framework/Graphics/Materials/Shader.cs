using System.Collections.Immutable;

namespace Vit.Framework.Graphics.Materials;

public class Shader {
	public readonly ImmutableArray<ShaderPart> Parts;
	public Shader ( ReadOnlySpan<ShaderPart> parts ) {
		Parts = parts.ToImmutableArray();
	}
}
