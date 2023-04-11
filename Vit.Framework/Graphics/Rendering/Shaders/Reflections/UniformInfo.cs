using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class UniformInfo {
	public readonly Dictionary<uint, UniformSetInfo> Sets = new();

	public unsafe void ParseSpirv ( spvc_compiler compiler, spvc_resources resources, spvc_resource_type resourceType ) {
		spvc_reflected_resource* list = default;
		nuint count = default;

		SPIRV.spvc_resources_get_resource_list_for_type( resources, resourceType, (spvc_reflected_resource*)&list, &count );
		for ( nuint i = 0; i < count; i++ ) {
			var res = list[i];
			var set = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)res.id, SpvDecoration.SpvDecorationDescriptorSet );

			if ( !Sets.TryGetValue( set, out var setInfo ) )
				Sets.Add( set, setInfo = new() );

			var resource = new UniformResourceInfo();
			resource.ParseSpriv( compiler, res );
			setInfo.Resources.Add( resource );
		}
	}

	public override string ToString () {
		return $"<\n\t{string.Join( "\n", Sets.Select( x => $"Set {x.Key} {x.Value}" ) ).Replace( "\n", "\n\t" )}\n>";
	}
}
