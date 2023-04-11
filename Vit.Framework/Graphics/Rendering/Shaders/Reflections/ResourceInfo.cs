using SPIRVCross;
using Vit.Framework.Interop;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class ResourceInfo {
	public string Name = string.Empty;
	public DataTypeInfo Type = DataTypeInfo.Void;

	public virtual unsafe void ParseSpriv ( spvc_compiler compiler, spvc_reflected_resource resource ) {
		var name = SPIRV.spvc_compiler_get_name( compiler, (SpvId)resource.id );
		Name = ( (CString)name ).ToString();
		var type = SPIRV.spvc_compiler_get_type_handle( compiler, resource.type_id );
		Type = DataTypeInfo.FromSpirv( compiler, type );
	}

	public override string ToString () {
		return $"{Type} {Name}";
	}
}
