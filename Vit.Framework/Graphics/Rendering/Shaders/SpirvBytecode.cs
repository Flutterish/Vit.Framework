using SPIRVCross;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Interop;
using Vortice.ShaderCompiler;

namespace Vit.Framework.Graphics.Rendering.Shaders;

public class SpirvBytecode { // TODO dispose this at some point?
	Result data;
	public Span<byte> Data => data.GetBytecode();
	public readonly ShaderPartType Type;
	public readonly string Identifier;
	public readonly string EntryPoint;
	public readonly ShaderInfo Reflections;

	public unsafe SpirvBytecode ( string source, ShaderLanguage language, ShaderPartType type, string identifier = "", string? entryPoint = null ) {
		Options spirvOptions = new();
		spirvOptions.SetSourceLanguage( language switch {
			ShaderLanguage.GLSL => SourceLanguage.GLSL,
			ShaderLanguage.HLSL => SourceLanguage.HLSL,
			_ => throw new ArgumentException( $"Invalid shader language: {language}", nameof( language ) )
		} );
		var spirvCompiler = new Compiler( spirvOptions );

		var shader = spirvCompiler.Compile( source, identifier, type switch {
			ShaderPartType.Vertex => ShaderKind.VertexShader,
			ShaderPartType.Fragment => ShaderKind.FragmentShader,
			ShaderPartType.Compute => ShaderKind.ComputeShader,
			_ => throw new ArgumentException( $"Invalid shader type: {type}", nameof( type ) )
		}, EntryPoint = entryPoint ?? (language, type) switch {
			(ShaderLanguage.GLSL, _ ) => "main",
			(ShaderLanguage.HLSL, ShaderPartType.Vertex ) => "vs_main",
			(ShaderLanguage.HLSL, ShaderPartType.Fragment ) => "ps_main",
			_ => "main"
		} );

		data = shader;
		Type = type;
		Identifier = identifier;

		if ( shader.Status != CompilationStatus.Success ) {
			throw new Exception( $"Shader compilation failed: {shader.ErrorMessage}" );
		}

		Reflections = ShaderInfo.FromSpirv( type, this ); // TODO merge with cross compile when applicable
	}

	public unsafe string CrossCompile ( ShaderLanguage target, Action<spvc_compiler>? action = null ) {
		void validate ( spvc_result result ) {
			if ( result != spvc_result.SPVC_SUCCESS ) {
				throw new Exception( $"{result}" );
			}
		}

		spvc_context context;
		validate( SPIRV.spvc_context_create( &context ) );
		static void ErrorCallback ( void* userData, byte* error ) {
			throw new Exception( ( (CString)error ).ToString() );
		}
		spvc_error_callback callback = new( Marshal.GetFunctionPointerForDelegate( ErrorCallback ) );

		SPIRV.spvc_context_set_error_callback( context, callback, null );
		spvc_parsed_ir ir;
		validate( SPIRV.spvc_context_parse_spirv( context, MemoryMarshal.Cast<byte, SpvId>( Data ).Data(), (nuint)Data.Length / 4, &ir ) );
		
		spvc_compiler compiler;
		validate( SPIRV.spvc_context_create_compiler( context, target switch {
			ShaderLanguage.GLSL => spvc_backend.Glsl,
			ShaderLanguage.HLSL => spvc_backend.Hlsl,
			_ => throw new ArgumentException( $"Invalid shader target: {target}", nameof( target ) )
		}, ir, spvc_capture_mode.TakeOwnership, &compiler ) );

		spvc_compiler_options options = default;
		SPIRV.spvc_compiler_create_compiler_options( compiler, &options );
		if ( target == ShaderLanguage.GLSL ) {
			SPIRV.spvc_compiler_options_set_uint( options, spvc_compiler_option.GlslVersion, 460 );
			SPIRV.spvc_compiler_options_set_bool( options, spvc_compiler_option.GlslEs, false );
		}
		else if ( target == ShaderLanguage.HLSL ) {
			SPIRV.spvc_compiler_options_set_uint( options, spvc_compiler_option.HlslShaderModel, 50 );
		}

		SPIRV.spvc_compiler_install_compiler_options( compiler, options );
		action?.Invoke( compiler );

		byte* res = null;
		validate( SPIRV.spvc_compiler_compile( compiler, (byte*)&res ) );
		var str = (new CString( res )).ToString();

		SPIRV.spvc_context_destroy( context );

		return str;
	}
}
