using SPIRVCross;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class StructTypeInfo : TypeInfo {
	public readonly uint Size;
	public readonly bool IsUnsized;
	public readonly string Name;
	public readonly List<StructMemberInfo> Members = new();

	public StructTypeInfo ( string name, uint size, bool unsized = false ) {
		Name = name;
		Size = size;
		IsUnsized = unsized;
	}

	public static unsafe StructTypeInfo FromSpirv ( spvc_compiler compiler, spvc_type type ) {
		var baseTypeId = SPIRV.spvc_type_get_base_type_id( type );
		var baseType = SPIRV.spvc_compiler_get_type_handle( compiler, baseTypeId );
		var name = (CString)SPIRV.spvc_compiler_get_name( compiler, (SpvId)baseTypeId );
		nuint size = 0;
		bool unsized = false;
		SPIRV.spvc_compiler_get_declared_struct_size( compiler, baseType, &size );
		if ( size == 0 ) {
			SPIRV.spvc_compiler_get_declared_struct_size_runtime_array( compiler, baseType, 1, &size );
			unsized = true;
		}
		var layout = new StructTypeInfo( name, (uint)size, unsized );

		var memberCount = SPIRV.spvc_type_get_num_member_types( baseType );
		for ( uint i = 0; i < memberCount; i++ ) {
			uint memberOffset = 0;
			SPIRV.spvc_compiler_type_struct_member_offset( compiler, baseType, i, &memberOffset );
			var memberName = (CString)SPIRV.spvc_compiler_get_member_name( compiler, baseTypeId, i );
			var memeberTypeId = SPIRV.spvc_type_get_member_type( baseType, i );
			var memberType = SPIRV.spvc_compiler_get_type_handle( compiler, memeberTypeId );
			layout.Members.Add( new( memberName, DataTypeInfo.FromSpirv( compiler, memberType ), memberOffset ) );
		}

		return layout;
	}

	public override string ToString () {
		return $"{( IsUnsized ? "buffer" : $"[Size: {Size}\t]")} {Name} {{\n\t{string.Join("\n", Members).Replace("\n", "\n\t")}\n}}";
	}
}

public class StructMemberInfo {
	public readonly uint Offset;
	public readonly string Name;
	public readonly DataTypeInfo Type;

	public StructMemberInfo ( string name, DataTypeInfo type, uint offset ) {
		Name = name;
		Type = type;
		Offset = offset;
	}

	public override string ToString () {
		return $"[Offset: {Offset}\t] {Type} {Name}";
	}
}