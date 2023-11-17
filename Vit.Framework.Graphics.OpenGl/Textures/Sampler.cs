using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Sampler : DisposableObject, ISampler {
	public readonly int Handle;
	public Sampler () {
		Handle = GL.GenSampler();
		GL.SamplerParameter( Handle, SamplerParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureBorderColor, new float[] { 1, 1, 1, 1 } );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureMagFilter, (int)TextureMagFilter.Linear );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureMinFilter, (int)TextureMinFilter.Linear );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteSampler( Handle );
	}
}
