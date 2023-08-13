using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformSetPool : IUniformSetPool {
	UniformLayout layout;
	Stack<IUniformSet> uniforms = new();
	public UniformSetPool ( UniformLayout layout ) {
		this.layout = layout;
	}

	public IUniformSet Rent () {
		if ( !uniforms.TryPop( out var set ) ) {
			set = new UniformSet( layout );
			DebugMemoryAlignment.SetDebugData( set, layout.Type.Resources );
		}

		return set;
	}

	public void Free ( IUniformSet set ) {
		uniforms.Push( set );
	}

	public void Dispose () {
		foreach ( var i in uniforms ) {
			i.Dispose();
		}
	}
}
