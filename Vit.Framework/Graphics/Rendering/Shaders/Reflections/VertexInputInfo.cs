using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class VertexInputInfo {
	public readonly Dictionary<uint, VertexInfo> Sets = new();

	public unsafe static VertexInputInfo FromSpirv ( spvc_compiler compiler, spvc_resources resources ) {
		VertexInputInfo info = new();
		spvc_reflected_resource* list = default;
		nuint count = default;

		SPIRV.spvc_resources_get_resource_list_for_type( resources, spvc_resource_type.StageInput, (spvc_reflected_resource*)&list, &count );
		for ( nuint i = 0; i < count; i++ ) {
			var res = list[i];
			uint set = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)res.id, SpvDecoration.SpvDecorationDescriptorSet );
			if ( !info.Sets.TryGetValue( set, out var boundSet ) )
				info.Sets.Add( set, boundSet = new() );

			var resource = new VertexResourceInfo();
			resource.ParseSpriv( compiler, res );
			boundSet.Resources.Add( resource );
		}

		return info;
	}

	public override string ToString () {
		return $"<\n\t{string.Join("\n", Sets.Select( x => $"Set {x.Key} {x.Value}" )).Replace("\n", "\n\t")}\n>";
	}
}
