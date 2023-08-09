using SPIRVCross;
using System.Collections.Immutable;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class DataTypeInfo {
	public static DataTypeInfo Void { get; } = new DataTypeInfo( PrimitiveType.Void, false, Array.Empty<uint>() );

	public readonly PrimitiveType PrimitiveType;
	public readonly TypeInfo? Layout;
	public readonly ImmutableArray<uint> Dimensions;
	public readonly bool IsArray;

	public uint FlattendedDimensions => Dimensions.Aggregate( 1u, (a, b) => a * b );

	public DataTypeInfo ( PrimitiveType type, bool isArray, IEnumerable<uint> dimensions ) {
		PrimitiveType = type;
		IsArray = isArray;

		Dimensions = dimensions.Reverse().SkipWhile( x => x == 1 ).Reverse().ToImmutableArray();
	}

	public DataTypeInfo ( PrimitiveType type, TypeInfo layout, bool isArray, IEnumerable<uint> dimensions ) {
		PrimitiveType = type;
		Layout = layout;
		IsArray = isArray;

		Dimensions = dimensions.Reverse().SkipWhile( x => x == 1 ).Reverse().ToImmutableArray();
	}

	public override string ToString () {
		var type = Layout?.ToString() ?? PrimitiveType.ToString(); 

		if ( IsArray )
			return $"{type}{string.Join( "", Dimensions.Select( x => $"[{x}]" ) )}";

		if ( Dimensions.Length > 0 )
			return $"{type}[{string.Join( "x", Dimensions )}]";

		return type;
	}

	public static unsafe DataTypeInfo FromSpirv ( spvc_compiler compiler, spvc_type type ) {
		var name = SPIRV.spvc_type_get_basetype( type );
		var rows = SPIRV.spvc_type_get_vector_size( type );
		var columns = SPIRV.spvc_type_get_columns( type );

		//var dimensions = SPIRV.spvc_type_get_num_array_dimensions( type );
		//var sizes = Enumerable.Range( 0, (int)dimensions ).Select( x => SPIRV.spvc_type_get_array_dimension( type, (uint)x ) );

		if ( name == spvc_basetype.Struct ) {
			return new DataTypeInfo( PrimitiveType.Struct, StructTypeInfo.FromSpirv( compiler, type ), false, new[] { rows, columns } );
		}
		else if ( name == spvc_basetype.SampledImage ) {
			return new DataTypeInfo( PrimitiveType.Sampler, ImageTypeInfo.FromSpirv( compiler, type ), false, new[] { rows, columns } );
		}
		else {
			PrimitiveType primitive = name switch {
				spvc_basetype.Fp32 => PrimitiveType.Float32,
				spvc_basetype.SampledImage => PrimitiveType.Sampler,
				_ => throw new Exception( "invalid data type primitive" )
			};
			return new DataTypeInfo( primitive, false, new[] { rows, columns } );
		}
	}
}

public abstract class TypeInfo { }