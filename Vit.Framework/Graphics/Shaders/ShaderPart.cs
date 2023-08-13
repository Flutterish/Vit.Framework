using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Shaders;

public class ShaderPart : DisposableObject {
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

	protected override void Dispose ( bool disposing ) {
		Value?.Dispose();
	}
}
