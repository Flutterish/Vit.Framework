using SPIRVCross;

namespace Vit.Framework.Graphics.Rendering.Shaders.Reflections;

public class UniformResourceInfo : ResourceInfo {
	public spvc_resource_type ResourceType;
	public uint Binding;
	public uint BindingBinaryOffset;
	public HashSet<ShaderPartType> Stages = new();
	public override unsafe void ParseSpriv ( spvc_compiler compiler, spvc_reflected_resource resource ) {
		base.ParseSpriv( compiler, resource );

		Binding = SPIRV.spvc_compiler_get_decoration( compiler, (SpvId)resource.id, SpvDecoration.SpvDecorationBinding );
		uint offset = 0;
		SPIRV.spvc_compiler_get_binary_offset_for_decoration( compiler, Id, SpvDecoration.SpvDecorationBinding, &offset );
		BindingBinaryOffset = offset;
	}

	public override string ToString () {
		return $"layout(binding = {Binding}) {base.ToString()}";
	}
}
