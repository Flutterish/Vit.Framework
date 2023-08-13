using Vit.Framework.Graphics.Rendering.Shaders.Reflections;

namespace Vit.Framework.Graphics.Direct3D11.Uniforms;

public class UniformLayout {
	public readonly UniformSetInfo Type;
	public readonly uint[] BindingLookup;

	public readonly int FirstConstantBuffer;
	public readonly int ConstantBufferCount;

	public readonly int FirstSampler;
	public readonly int SamplerCount;

	public UniformLayout ( uint set, UniformSetInfo type, UniformFlatMapping mapping ) {
		Type = type;

		var lookup = mapping.CreateResourceLookup( set, type );
		BindingLookup = lookup.bindingLookup;

		foreach ( var (resourceType, _first, count) in lookup.resources ) {
			var first = (int)_first;
			switch ( resourceType ) {
				case SPIRVCross.spvc_resource_type.UniformBuffer:
					(FirstConstantBuffer, ConstantBufferCount) = (first, count);
					break;

				case SPIRVCross.spvc_resource_type.SampledImage:
					(FirstSampler, SamplerCount) = (first, count);
					break;

				default:
					break;
			}
		}
	}
}
