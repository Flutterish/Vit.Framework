using SPIRVCross;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Interop;
using Vit.Framework.Memory;
using Vortice.ShaderCompiler;

namespace Vit.Framework.Graphics.OpenGl.Shaders;

public class UnlinkedShader : DisposableObject, IShaderPart {
	public readonly SpirvBytecode Spirv;
	public UniformFlatMappingDictionary<Shader> LinkedShaders = new();
	public unsafe UnlinkedShader ( SpirvBytecode spirv ) {
		Spirv = spirv;
	}

	public Shader GetShader ( UniformFlatMapping mapping ) {
		if ( !LinkedShaders.TryGetValue( mapping, out var shader ) )
			LinkedShaders.Add( mapping, shader = new( Spirv, mapping ) );

		return shader;
	}

	public ShaderPartType Type => Spirv.Type;
	public ShaderInfo ShaderInfo => Spirv.Reflections;

	protected override void Dispose ( bool disposing ) {
		foreach ( var i in LinkedShaders ) {
			i.Value.Dispose();
		}
	}
}

public class Shader : DisposableObject {
	public readonly int Handle;
	public unsafe Shader ( SpirvBytecode spirv, UniformFlatMapping mapping ) {
		var handle = GL.CreateShader( spirv.Type switch {
			ShaderPartType.Vertex => ShaderType.VertexShader,
			ShaderPartType.Fragment => ShaderType.FragmentShader,
			ShaderPartType.Compute => ShaderType.ComputeShader,
			_ => throw new ArgumentException( $"Shader type not supported: {spirv.Type}", nameof( spirv ) )
		} );

		var dataArray = new RentedArray<byte>( spirv.Data );
		var wordView = MemoryMarshal.Cast<byte, uint>( dataArray.AsSpan() );
		foreach ( var ((set, oiginalBinding), binding) in mapping.Bindings ) {
			if ( !spirv.Reflections.Uniforms.Sets.TryGetValue( set, out var setInfo ) || setInfo.Resources.FirstOrDefault( x => x.Binding == oiginalBinding ) is not UniformResourceInfo resource )
				continue;

			wordView[(int)resource.BindingBinaryOffset] = binding;
		}

		GL.ShaderBinary( 1, &handle, BinaryFormat.ShaderBinaryFormatSpirV, (nint)dataArray.Data(), dataArray.Length );
		Handle = handle;

		GL.SpecializeShader( handle, spirv.EntryPoint, 0, (int*)null, (int*)null );

		GL.GetShader( handle, ShaderParameter.CompileStatus, out var status );
		if ( status == 0 ) {
			GL.GetShader( handle, ShaderParameter.InfoLogLength, out var length );
			GL.GetShaderInfoLog( handle, length, out _, out var info );
			throw new Exception( info );
		}
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteShader( Handle );
	}
}