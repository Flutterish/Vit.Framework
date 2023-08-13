using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Descriptions;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Shaders;

public class Shader : DisposableObject {
	public readonly VertexInputDescription? VertexInput;
	public readonly ImmutableArray<ShaderPart> Parts;
	public Shader ( ReadOnlySpan<ShaderPart> parts, VertexInputDescription? vertexInput ) {
		Parts = parts.ToImmutableArray();
		VertexInput = vertexInput;
	}

	/// <summary>
	/// The underlying shader set - guaranteed to be set on the draw thread.
	/// </summary>
	public IShaderSet Value = null!;

	public void Compile ( IRenderer renderer ) {
		if ( Value == null ) {
			foreach ( var i in Parts )
				i.Compile( renderer );
			Value = renderer.CreateShaderSet( Parts.Select( x => x.Value ), VertexInput );
		}
	}

	protected override void Dispose ( bool disposing ) {
		Value?.Dispose();
	}
}
