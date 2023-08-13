using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;
using Vit.Framework.Graphics.Rendering.Validation;

namespace Vit.Framework.Graphics.Software.Uniforms;

public class UniformSetPool : IUniformSetPool {
	UniformSetInfo type;
	Stack<IUniformSet> uniforms = new();
	public UniformSetPool ( UniformSetInfo type ) {
		this.type = type;
	}

	public IUniformSet Rent () {
		if ( !uniforms.TryPop( out var set ) ) {
			set = new UniformSet();
			DebugMemoryAlignment.SetDebugData( set, type.Resources );
		}

		return set;
	}

	public void Free ( IUniformSet set ) {
		((UniformSet)set).Free();
		uniforms.Push( set );
	}

	public void Dispose () {
		foreach ( var i in uniforms ) {
			i.Dispose();
		}
	}
}
