using SPIRVCross;
using System.Runtime.InteropServices;
using System.Text;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class ShaderInfo {
	public readonly ShaderPartType Type;
	public VertexInfo Input = new();
	public VertexInfo Output = new();
	public UniformInfo Uniforms = new();
	public UniformInfo Samplers = new();
	public UniformInfo Storage = new();

	public ShaderInfo ( ShaderPartType type ) {
		Type = type;
	}

	public static unsafe ShaderInfo FromSpirv ( ShaderPartType type, SpirvBytecode bytecode ) {
		void validate ( spvc_result result ) {
			if ( result != spvc_result.SPVC_SUCCESS ) {
				throw new Exception( $"{result}" );
			}
		}

		spvc_context context;
		validate( SPIRV.spvc_context_create( &context ) );
		static void ErrorCallback ( void* userData, byte* error ) {
			throw new Exception( ((CString)error).ToString() );
		}
		spvc_error_callback callback = new( Marshal.GetFunctionPointerForDelegate( ErrorCallback ) );

		SPIRV.spvc_context_set_error_callback( context, callback, null );
		spvc_parsed_ir ir;
		validate( SPIRV.spvc_context_parse_spirv( context, MemoryMarshal.Cast<byte, SpvId>( bytecode.Data ).Data(), (nuint)bytecode.Data.Length / 4, &ir ) );
		spvc_compiler compiler;
		validate( SPIRV.spvc_context_create_compiler( context, spvc_backend.Glsl, ir, spvc_capture_mode.TakeOwnership, &compiler ) );
		spvc_resources resources;
		SPIRV.spvc_compiler_create_shader_resources( compiler, &resources );

		var info = new ShaderInfo( type );
		info.Input.ParseSpirv( compiler, resources, spvc_resource_type.StageInput );
		info.Output.ParseSpirv( compiler, resources, spvc_resource_type.StageOutput );
		info.Uniforms.ParseSpirv( compiler, resources, spvc_resource_type.UniformBuffer );
		info.Samplers.ParseSpirv( compiler, resources, spvc_resource_type.SampledImage );
		info.Storage.ParseSpirv( compiler, resources, spvc_resource_type.StorageBuffer );

		SPIRV.spvc_context_destroy( context );

		if ( type == ShaderPartType.Compute ) {
			var k = info.ToString();
		}

		return info;
	}

	public override string ToString () {
		StringBuilder sb = new();
		sb.AppendLine( $"{Type} Shader {{" );
		if ( Input.Resources.Any() ) 
			sb.AppendLine( $"\tInput = {Input.ToString().Replace("\n", "\n\t")}" );
		if ( Output.Resources.Any() ) 
			sb.AppendLine( $"\tOutput = {Output.ToString().Replace("\n", "\n\t")}" );
		if ( Uniforms.Sets.Any() ) 
			sb.AppendLine( $"\tUniforms = {Uniforms.ToString().Replace("\n", "\n\t")}" );
		if ( Samplers.Sets.Any() )
			sb.AppendLine( $"\tSamplers = {Samplers.ToString().Replace("\n", "\n\t")}" );
		if ( Storage.Sets.Any() )
			sb.AppendLine( $"\tStorage = {Storage.ToString().Replace("\n", "\n\t")}" );
		sb.Append( "}" );
		return sb.ToString();
	}
}
