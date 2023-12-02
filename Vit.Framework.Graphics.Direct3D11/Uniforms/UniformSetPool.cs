using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformSetPool : DisposableObject, IUniformSetPool {
	UniformLayout layout;
	Stack<IUniformSet> uniforms = new();
	public UniformSetPool ( UniformLayout layout ) {
		this.layout = layout;
		DebugMemoryAlignment.SetDebugData( this, layout.Type.Resources );
	}

	public IUniformSet Rent () {
		if ( !uniforms.TryPop( out var set ) ) {
			set = new UniformSet( layout );
			DebugMemoryAlignment.SetDebugData( this, set );
		}

		return set;
	}

	public void Free ( IUniformSet set ) {
		uniforms.Push( set );
	}

	protected override void Dispose ( bool disposing ) {
		DebugMemoryAlignment.ClearDebugData( this );
	}
}
