using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class VertexResourceInfo : ResourceInfo {
	public uint Location;
	public override void ParseSpriv ( spvc_compiler compiler, spvc_reflected_resource resource ) {
		base.ParseSpriv( compiler, resource );

		Location = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationLocation );
	}

	public override string ToString () {
		return $"layout(location = {Location}) {base.ToString()}";
	}
}
