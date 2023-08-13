using Vit.Framework.Graphics.Rendering.Shaders.Reflections;

namespace Vit.Framework.Graphics.OpenGl.Uniforms;

public class UniformLayout {
	public readonly uint[] BindingLookup;

	public readonly uint FirstUbo;
	public readonly int UboCount;

	public readonly uint FirstSampler;
	public readonly int SamplerCount;

	public UniformLayout ( uint set, UniformSetInfo type, UniformFlatMapping mapping ) {
		BindingLookup = mapping.CreateBindingLookup( set );

		(FirstUbo, UboCount) = mapping.GetResourceArraySize( set, type, SPIRVCross.spvc_resource_type.UniformBuffer );
		(FirstSampler, SamplerCount) = mapping.GetResourceArraySize( set, type, SPIRVCross.spvc_resource_type.SampledImage );
	}
}
