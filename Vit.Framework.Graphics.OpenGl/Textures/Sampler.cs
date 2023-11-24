using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Memory;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace Vit.Framework.Graphics.OpenGl.Textures;

public class Sampler : DisposableObject, ISampler {
	public readonly int Handle;
	static readonly float[] transparentBlack = new[] { 0f, 0f, 0f, 0f };
	public unsafe Sampler ( SamplerDescription description ) {
		Handle = GL.GenSampler();

		static int wrapMode ( Graphics.Rendering.Textures.TextureWrapMode wrapMode ) {
			return (int)(wrapMode switch {
				Graphics.Rendering.Textures.TextureWrapMode.Repeat => TextureWrapMode.Repeat,
				Graphics.Rendering.Textures.TextureWrapMode.MirroredRepeat => TextureWrapMode.MirroredRepeat,
				Graphics.Rendering.Textures.TextureWrapMode.ClampToEdge => TextureWrapMode.ClampToEdge,
				Graphics.Rendering.Textures.TextureWrapMode.TransparentBlackBorder or _ => TextureWrapMode.ClampToBorder
			});
		}
		
		GL.SamplerParameter( Handle, SamplerParameterName.TextureWrapS, wrapMode( description.WrapU ) );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureWrapT, wrapMode( description.WrapV ) );
		if ( description.UsesBorderColour ) {
			GL.SamplerParameter( Handle, SamplerParameterName.TextureBorderColor, transparentBlack );
		}
		GL.SamplerParameter( Handle, SamplerParameterName.TextureMagFilter, (int)(description.MagnificationFilter switch {
			FilteringMode.Nearest => TextureMagFilter.Nearest,
			FilteringMode.Linear or _ => TextureMagFilter.Linear
		}) );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureMinFilter, (int)((description.MinificationFilter, description.MipmapMode) switch {
			(FilteringMode.Nearest, MipmapMode.None) => TextureMinFilter.Nearest,
			(FilteringMode.Linear, MipmapMode.None) => TextureMinFilter.Linear,
			(FilteringMode.Nearest, MipmapMode.Nearest) => TextureMinFilter.NearestMipmapNearest,
			(FilteringMode.Linear, MipmapMode.Nearest) => TextureMinFilter.LinearMipmapNearest,
			(FilteringMode.Nearest, MipmapMode.Linear) => TextureMinFilter.NearestMipmapLinear,
			(FilteringMode.Linear, MipmapMode.Linear) or _ => TextureMinFilter.LinearMipmapLinear
		}) );
		if ( description.EnableAnisotropy ) {
			GL.SamplerParameter( Handle, SamplerParameterName.TextureMaxAnisotropyExt, description.MaximumAnisotropicFiltering );
		}

		GL.SamplerParameter( Handle, SamplerParameterName.TextureMinLod, description.MinimimMipmapLevel );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureMaxLod, description.MaximimMipmapLevel );
		GL.SamplerParameter( Handle, SamplerParameterName.TextureLodBias, description.MipmapLevelBias );
	}

	protected override void Dispose ( bool disposing ) {
		GL.DeleteSampler( Handle );
	}
}
