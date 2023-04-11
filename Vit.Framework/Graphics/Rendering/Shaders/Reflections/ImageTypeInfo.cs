using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class ImageTypeInfo : TypeInfo {
	public readonly DataTypeInfo Format;
	public readonly uint Dimensions;

	public ImageTypeInfo ( DataTypeInfo format, uint dimensions ) {
		Format = format;
		Dimensions = dimensions;
	}

	public static unsafe ImageTypeInfo FromSpirv ( spvc_compiler compiler, spvc_type type ) {
		var format = SPIRV.spvc_compiler_get_type_handle( compiler, SPIRV.spvc_type_get_image_sampled_type( type ) );
		var dimensions = SPIRV.spvc_type_get_image_dimension( type );

		return new ImageTypeInfo( DataTypeInfo.FromSpirv( compiler, format ), dimensions switch {
			SpvDim.SpvDim1D => 1,
			SpvDim.SpvDim2D => 2,
			SpvDim.SpvDim3D => 3,
			_ => throw new Exception( "unsupported dimension" )
		} );
	}

	public override string ToString () {
		return $"Sampler{Dimensions}D<{Format}>";
	}
}
