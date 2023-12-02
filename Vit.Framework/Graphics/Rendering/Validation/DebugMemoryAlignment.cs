using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering.Shaders;
using Vit.Framework.Graphics.Rendering.Shaders.Reflections;
using Vit.Framework.Graphics.Rendering.Uniforms;

namespace Vit.Framework.Graphics.Rendering.Validation;

public static class DebugMemoryAlignment {
	static Dictionary<IUniformSetPool, (List<IUniformSet> sets, Dictionary<uint, DataTypeInfo> dataTypeByBinding)> pools = new();
	static Dictionary<IUniformSet, IUniformSetPool> poolBySet = new();

	public static void SetDebugData ( IUniformSetPool pool, IEnumerable<UniformResourceInfo> uniforms ) {
		Dictionary<uint, DataTypeInfo> dict = new();
		lock ( pools ) {
			pools.Add( pool, (new(), dict) );
		}

		foreach ( var i in uniforms.GroupBy( x => x.Binding ) ) {
			var rep = i.First();
			dict.Add( rep.Binding, rep.Type );
		}
	}

	[Conditional( "DEBUG" )]
	public static void SetDebugData ( IUniformSetPool pool, IUniformSet set ) {
		lock ( pools ) {
			poolBySet.Add( set, pool );
			pools[pool].sets.Add( set );
		}
	}

	[Conditional( "DEBUG" )]
	public static void ClearDebugData ( IUniformSetPool pool ) {
		lock ( pools ) {
			pools.Remove( pool, out var data );
			foreach ( var i in data.sets ) {
				poolBySet.Remove( i );
			}
		}
	}

	[Conditional( "DEBUG" )]
	public static void AssertStructAlignment ( IUniformSet set, uint binding, Type type ) {
		AssertStructAlignment( type, pools[poolBySet[set]].dataTypeByBinding[binding] );
	}

	static HashSet<(Type, DataTypeInfo)> @checked = new();
	[Conditional( "DEBUG" )]
	public static void AssertStructAlignment ( Type type, DataTypeInfo dataType ) {
		lock ( @checked ) {
			if ( !@checked.Add( (type, dataType) ) )
				return;
		}

		int SizeOf ( Type type ) {
			return Marshal.SizeOf( Activator.CreateInstance( type )! );
		}

		if ( dataType.PrimitiveType == PrimitiveType.Struct ) { // ex. Buffer<UniformBlock>
			var size = SizeOf( type );
			var layout = (StructTypeInfo)dataType.Layout!;
			Debug.Assert( size == layout.Size, $"Marshalled data type must match size (expected {layout.Size}, was {size}) at {type.Name} ~ {dataType}" );
			validateStruct( type, layout, type.Name );
		}

		void validateStruct ( Type type, StructTypeInfo layout, string name ) {
			Debug.Assert( type.IsValueType, $"Marshalled data type must be an unmanaged struct (at {name} ~ {dataType})" );

			var expectedFields = layout.Members;
			var fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).ToArray();
			Debug.Assert( fields.Length == expectedFields.Count, $"Marshalled data type must match field count (expected {expectedFields.Count}, was {fields.Length}) at {name} ~ {dataType}" );

			foreach ( var (field, expected) in fields.Zip( expectedFields ) ) {
				if ( expected.Type.IsArray ) {
					throw new NotImplementedException();
				}

				var offset = Marshal.OffsetOf( type, field.Name );
				var size = SizeOf( field.FieldType );
				var fieldName = $"{name}.{field.Name}";

				if ( offset != expected.Offset ) {
					Debug.Fail( $"Marshalled field offset must match shader (expected {expected.Offset}, was {offset}) at {fieldName} in {dataType}\nRemember that in shaders uniforms and structs, vectors and matrice rows are stored in 4-wide slots. It is possible that you need to pad before this member." );
				}
				if ( size < expected.Size ) { // bigger size is fine, it might just be padding
					failSizeCheck( field.FieldType, expected.Type, fieldName, $"Marshalled field must match size (expected {expected.Size}, was {size}) at {fieldName} in {dataType}" );
				}

				if ( expected.Type.PrimitiveType == PrimitiveType.Struct ) {
					validateStruct( field.FieldType, (StructTypeInfo)expected.Type.Layout!, fieldName );
				}
			}
		}

		void failSizeCheck ( Type type, DataTypeInfo dataType, string name, string message ) {
			if ( dataType.FlattendedDimensions == 1 )
				message += "\nYou probably used the wrong data type, or a vector/matrix instead of a single variable.";
			if ( dataType.Dimensions.Length == 1 )
				message += "\nYou probably used the wrong vector length.";
			if ( dataType.Dimensions.Length == 2 )
				message += "\nYou probably used the wrong matrix size. Remember that in shaders uniforms and structs matrices are stored with 4 columns.";

			Debug.Fail( message );
		}
	}
}
