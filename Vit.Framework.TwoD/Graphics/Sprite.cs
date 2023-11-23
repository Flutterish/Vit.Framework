using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public class Sprite : TexturedQuad {
	Texture texture = null!;

	SharedResourceInvalidations textureInvalidations;
	public Texture Texture {
		get => texture;
		set {
			if ( texture == value )
				return;

			texture = value;
			textureInvalidations.Invalidate();
			InvalidateDrawNodes();
		}
	}

	protected override void OnLoad ( IReadOnlyDependencyCache deps ) {
		base.OnLoad( deps );
		texture ??= deps.Resolve<TextureStore>().GetTexture( TextureStore.WhitePixel );
	}

	protected override Rendering.DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return new DrawNode( this, subtreeIndex );
	}

	class DrawNode : TexturedQuad.DrawNode<Sprite> {
		public DrawNode ( Sprite source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		Texture texture = null!;
		SharedResourceUpload textureUpload;

		protected override void UpdateState () {
			base.UpdateState();
			texture = Source.texture;
			textureUpload = Source.textureInvalidations.GetUpload();
		}

		protected override bool UpdateTexture ( [NotNullWhen(true)] out ITexture2DView? texture, [NotNullWhen(true)] out ISampler? sampler ) {
			texture = this.texture.View;
			sampler = this.texture.Sampler;
			return textureUpload.Validate( ref Source.textureInvalidations );
		}
	}
}
