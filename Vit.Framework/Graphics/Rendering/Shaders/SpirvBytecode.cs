using SPIRVCross;
using System.Runtime.InteropServices;
using Vit.Framework.Interop;
using Vortice.ShaderCompiler;

namespace Vit.Framework.Graphics.Rendering.Shaders;

public class SpirvBytecode {
	Result data;
	public Span<byte> Data => data.GetBytecode();
	public readonly ShaderPartType Type;
	public readonly string Identifier;
	public readonly string EntryPoint;

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
			throw new Exception( "Shader compilation failed" );
		}

		//reflect();
	}

	unsafe void reflect () {
		void validate ( spvc_result result ) {
			if ( result != spvc_result.SPVC_SUCCESS ) {
				throw new Exception( $"{result}" );
			}
		}

		spvc_context context;
		validate( SPIRV.spvc_context_create( &context ) );
		static void ErrorCallback ( void* userData, byte* error ) {
			Console.WriteLine( (CString)error );
		}
		spvc_error_callback callback = new( Marshal.GetFunctionPointerForDelegate( ErrorCallback ) );

		SPIRV.spvc_context_set_error_callback( context, callback, null );
		spvc_parsed_ir ir;
		validate( SPIRV.spvc_context_parse_spirv( context, MemoryMarshal.Cast<byte, SpvId>( Data ).Data(), (nuint)Data.Length / 4, &ir ) );
		spvc_compiler compiler;
		validate( SPIRV.spvc_context_create_compiler( context, spvc_backend.Glsl, ir, spvc_capture_mode.TakeOwnership, &compiler ) );
		spvc_resources resources;
		SPIRV.spvc_compiler_create_shader_resources( compiler, &resources );

		spvc_reflected_resource* list = default;
		nuint count = default;

		Console.WriteLine( $"\n\nReflecting {Type} Shader" );
		foreach ( var resourceType in Enum.GetValues<spvc_resource_type>().Except( new[] { spvc_resource_type.Unknown, spvc_resource_type.RayQuery, spvc_resource_type.IntMax } ) ) {
			SPIRV.spvc_resources_get_resource_list_for_type( resources, resourceType, (spvc_reflected_resource*)&list, &count );
			if ( count == 0 )
				continue;

			Console.WriteLine( $"Type: {resourceType}" );
			for ( nuint i = 0; i < count; i++ ) {
				var res = list[i];
				var baseType = SPIRV.spvc_compiler_get_type_handle( compiler, res.base_type_id );
				var type = SPIRV.spvc_compiler_get_type_handle( compiler, res.type_id );
				var baseTypeName = SPIRV.spvc_type_get_basetype( baseType );
				var typeName = SPIRV.spvc_type_get_basetype( type );
				var baseTypeSize = SPIRV.spvc_type_get_vector_size( baseType );
				var typeSize = SPIRV.spvc_type_get_vector_size( type );
				Console.WriteLine( $"\tID: {res.id}, BaseType ({res.base_type_id}): {baseTypeName}[{baseTypeSize}], Type ({res.type_id}): {typeName}[{typeSize}], Name: {(CString)res.name}" );

				//uint set = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)res.id, SpvDecoration.SpvDecorationDescriptorSet );
				//Console.WriteLine( $"\tSet: {set}" );
				//uint binding = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)res.id, SpvDecoration.SpvDecorationBinding );
				//Console.WriteLine( $"\tBinding: {binding}" );
				
				if ( i + 1 != count )
					Console.WriteLine( "\t--------" );
			}
		}

		SPIRV.spvc_context_destroy( context );
	}
}
