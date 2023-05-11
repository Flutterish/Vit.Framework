using SPIRVCross;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Memory;
using Vortice.Direct3D11;

namespace Vit.Framework.Graphics.Direct3D11.Shaders;

public class UnlinkedShader : DisposableObject, IShaderPart {
	public ShaderPartType Type => bytecode.Type;
	public ShaderInfo ShaderInfo => bytecode.Reflections;

	SpirvBytecode bytecode;
	public readonly ID3D11Device Device;
	public UnlinkedShader ( SpirvBytecode bytecode, ID3D11Device device ) {
		this.bytecode = bytecode;
		Device = device;
	}

	public UniformFlatMappingDictionary<Shader> LinkedShaders = new();
	public Shader GetShader ( UniformFlatMapping mapping ) {
		if ( !LinkedShaders.TryGetValue( mapping, out var shader ) )
			LinkedShaders.Add( mapping, shader = bytecode.Type switch {
				ShaderPartType.Vertex => new VertexShader( bytecode, Device, mapping ),
				ShaderPartType.Fragment => new PixelShader( bytecode, Device, mapping ),
				_ => throw new Exception( $"Unsupported shader type: {bytecode.Type}" )
			} );

		return shader;
	}

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in LinkedShaders ) {
			i.Value.Dispose();
		}
	}
}

public abstract class Shader : DisposableObject {
	protected static unsafe string CrossCompile ( SpirvBytecode bytecode, UniformFlatMapping mapping ) {
		return bytecode.CrossCompile( ShaderLanguage.HLSL, compiler => {
			foreach ( var ((set, originalBinding), binding) in mapping.Bindings ) {
				if ( !bytecode.Reflections.Uniforms.Sets.TryGetValue( set, out var setInfo ) || setInfo.Resources.FirstOrDefault( x => x.Binding == originalBinding ) is not UniformResourceInfo resource )
					continue;

				SPIRV.spvc_compiler_set_decoration( compiler, resource.Id, SpvDecoration.SpvDecorationBinding, binding );
			}
		} );
	}

	public abstract void Bind ( ID3D11DeviceContext context );
}

public class PixelShader : Shader {
	public readonly ID3D11PixelShader Handle;
	public PixelShader ( SpirvBytecode bytecode, ID3D11Device device, UniformFlatMapping mapping ) {
		var crossCompiled = CrossCompile( bytecode, mapping );
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
	public VertexShader ( SpirvBytecode bytecode, ID3D11Device device, UniformFlatMapping mapping ) {
		var crossCompiled = CrossCompile( bytecode, mapping );
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
