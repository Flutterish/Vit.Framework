using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class VertexInfo {
	public readonly List<VertexResourceInfo> Resources = new();

	public unsafe void ParseSpirv ( spvc_compiler compiler, spvc_resources resources, spvc_resource_type resourceType ) {
		spvc_reflected_resource* list = default;
		nuint count = default;

		SPIRV.spvc_resources_get_resource_list_for_type( resources, resourceType, (spvc_reflected_resource*)&list, &count );
		for ( nuint i = 0; i < count; i++ ) {
			var res = list[i];

			var resource = new VertexResourceInfo();
			resource.ParseSpriv( compiler, res );
			Resources.Add( resource );
		}
	}

	public override string ToString () {
		return $"[\n\t{string.Join("\n", Resources).Replace("\n", "\n\t")}\n]";
	}
}
