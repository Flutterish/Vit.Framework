using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Interop;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class Shader : DisposableObject, IShaderPart {
	public readonly SpirvBytecode Spirv;
	public readonly int Handle;

	public unsafe Shader ( SpirvBytecode spirv ) {
		Spirv = spirv;

		var handle = GL.CreateShader( spirv.Type switch {
			ShaderPartType.Vertex => ShaderType.VertexShader,
			ShaderPartType.Fragment => ShaderType.FragmentShader,
			ShaderPartType.Compute => ShaderType.ComputeShader,
			_ => throw new ArgumentException( $"Shader type not supported: {spirv.Type}", nameof(spirv) )
		} );
		GL.ShaderBinary( 1, &handle, BinaryFormat.ShaderBinaryFormatSpirV, (nint)spirv.Data.Data(), spirv.Data.Length );
		Handle = handle;

		GL.SpecializeShader( handle, spirv.EntryPoint, 0, (int*)null, (int*)null );

		GL.GetShader( handle, ShaderParameter.CompileStatus, out var status );
		if ( status == 0 ) {
			GL.GetShader( handle, ShaderParameter.InfoLogLength, out var length );
			GL.GetShaderInfoLog( handle, length, out _, out var info );
			throw new Exception( info );
		}
	}

	public ShaderPartType Type => Spirv.Type;
	public ShaderInfo ShaderInfo => Spirv.Reflections;

	protected override void Dispose ( bool disposing ) {
		GL.DeleteShader( Handle );
	}
}
