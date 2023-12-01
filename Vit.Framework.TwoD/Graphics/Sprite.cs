using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Graphics.Textures;

namespace Vit.Framework.TwoD.Graphics;

public class Sprite : TexturedQuad {
	Texture texture = null!;

	public Texture Texture {
		get => texture;
		set {
			if ( texture == value )
				return;

			texture = value;
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
		protected override void UpdateState () {
			base.UpdateState();
			texture = Source.texture;
		}

		public override (ITexture2DView, ISampler) GetTextureSampler ( IRenderer renderer )
			=> (texture.View, texture.Sampler);
	}
}
