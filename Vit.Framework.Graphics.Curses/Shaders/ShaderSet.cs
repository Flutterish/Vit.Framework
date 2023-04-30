using System.Collections.Immutable;
using Vit.Framework.Graphics.Rendering.Shaders;

namespace Vit.Framework.Graphics.Curses.Shaders;

public class ShaderSet : IShaderSet {
	public IEnumerable<IShaderPart> Parts => Shaders;
	public readonly ImmutableArray<Shader> Shaders;
	public ShaderSet ( IEnumerable<IShaderPart> parts ) {
		Shaders = parts.Select( x => (Shader)x ).ToImmutableArray();
	}


	public void Dispose () {
		
	}
}
