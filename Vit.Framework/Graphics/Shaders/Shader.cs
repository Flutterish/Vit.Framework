using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Shaders;

public class Shader : DisposableObject {
	public readonly ImmutableArray<ShaderPart> Parts;
	public Shader ( ReadOnlySpan<ShaderPart> parts ) {
		Parts = parts.ToImmutableArray();
	}

	/// <summary>
	/// The underlying shader set - guaranteed to be set on the draw thread.
	/// </summary>
	public IShaderSet Value = null!;

	public void Compile ( IRenderer renderer ) {
		Value = renderer.CreateShaderSet( Parts.Select( x => x.Value ) );
	}

	protected override void Dispose ( bool disposing ) {
		Value?.Dispose();
	}
}
