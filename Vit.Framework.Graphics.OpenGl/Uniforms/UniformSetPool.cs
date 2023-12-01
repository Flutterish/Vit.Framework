using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformSetPool : DisposableObject, IUniformSetPool {
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

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in uniforms ) {
			i.Dispose();
		}
	}
}
