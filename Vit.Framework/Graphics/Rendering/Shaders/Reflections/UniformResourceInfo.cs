using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class UniformResourceInfo : ResourceInfo {
	public uint Binding;
	public override void ParseSpriv ( spvc_compiler compiler, spvc_reflected_resource resource ) {
		base.ParseSpriv( compiler, resource );

		Binding = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationBinding );
	}

	public override string ToString () {
		return $"layout(binding = {Binding}) {base.ToString()}";
	}
}
