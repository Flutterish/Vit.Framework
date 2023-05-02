using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Shaders;

public abstract class Shader : DisposableObject, IShaderPart {
	public ShaderPartType Type => bytecode.Type;
	public ShaderInfo ShaderInfo => bytecode.Reflections;

	SpirvBytecode bytecode;
	protected Shader ( SpirvBytecode bytecode ) {
		this.bytecode = bytecode;
	}

	public abstract void Bind ( ID3D11DeviceContext context );
	protected abstract override void Dispose ( bool disposing );
}

public class PixelShader : Shader {
	public readonly ID3D11PixelShader Handle;
	public PixelShader ( SpirvBytecode bytecode, ID3D11Device device ) : base( bytecode ) {
		var crossCompiled = bytecode.CrossCompile( ShaderLanguage.HLSL );
		var data = Vortice.D3DCompiler.Compiler.Compile( crossCompiled, "main", "", "ps_5_0" );
		Handle = device.CreatePixelShader( data.Span );
	}

	public override void Bind ( ID3D11DeviceContext context ) {
		context.PSSetShader( Handle );
	}

	protected override void Dispose ( bool disposing ) {
		Handle.Dispose();
	}
}

public class VertexShader : Shader {
	public readonly ID3D11VertexShader Handle;
	public readonly ReadOnlyMemory<byte> Source;
	public VertexShader ( SpirvBytecode bytecode, ID3D11Device device ) : base( bytecode ) {
		var crossCompiled = bytecode.CrossCompile( ShaderLanguage.HLSL );
		var data = Source = Vortice.D3DCompiler.Compiler.Compile( crossCompiled, "main", "", "vs_5_0" );
		Handle = device.CreateVertexShader( data.Span );
	}

	public override void Bind ( ID3D11DeviceContext context ) {
		context.VSSetShader( Handle );
	}

	protected override void Dispose ( bool disposing ) {
		Handle.Dispose();
	}
}
