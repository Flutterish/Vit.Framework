using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;

namespace Vit.Framework.Graphics.Shaders;

public class ShaderPart : IDisposable {
	public readonly SpirvBytecode Bytecode;
	public ShaderPart ( SpirvBytecode bytecode ) {
		Bytecode = bytecode;
	}

	/// <summary>
	/// The underlying shader part - guaranteed to be set on the draw thread.
	/// </summary>
	public IShaderPart Value = null!;

	public void Compile ( IRenderer renderer ) {
		if ( Value == null )
			Value = renderer.CompileShaderPart( Bytecode );
	}

	public void Dispose () {
		Value?.Dispose();
		Value = null!;
	}
}
