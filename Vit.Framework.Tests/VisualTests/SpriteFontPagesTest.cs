using System.Diagnostics.CodeAnalysis;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Text.Fonts;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Graphics.Text;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Input;

namespace Vit.Framework.Tests.VisualTests;

public class SpriteFontPagesTest : TestScene {
	public SpriteFontPagesTest () {
		SpriteFontDisplay display;
		AddChild( new Visual<SpriteFontDisplay>( display = new() ), new() {
			Size = new(1024)
		} );

		AddChild( new BasicButton {
			Text = "Previous Page",
			Clicked = () => {
				display.PageId = int.Max( display.PageId - 1, 0 );
			}
		}, new() {
			Size = (300, 100),
			Anchor = Anchor.TopLeft,
			Origin = Anchor.TopLeft
		} );

		AddChild( new BasicButton {
			Text = "Next Page",
			Clicked = () => {
				display.PageId++;
			}
		}, new() {
			Size = (300, 100),
			Anchor = Anchor<float>.TopLeft + new RelativeAxes2<float>( 320, 0 ),
			Origin = Anchor.TopLeft
		} );
	}

	class SpriteFontDisplay : TexturedQuad {
		Font font = null!;
		SpriteFontStore spriteFontStore = null!;

		int pageId;
		public int PageId {
			get => pageId;
			set {
				pageId = value;
				InvalidateDrawNodes();
			}
		}

		protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
			base.OnLoad( dependencies );

			var fonts = dependencies.Resolve<FontStore>();
			spriteFontStore = dependencies.Resolve<SpriteFontStore>();
			font = fonts.GetFont( FrameworkUIScheme.EmojiFont );
		}

		protected override DrawNode CreateDrawNode<TRenderer> ( int subtreeIndex ) {
			return new DrawNode( this, subtreeIndex );
		}

		protected class DrawNode : DrawNode<SpriteFontDisplay> {
			Font font;
			SpriteFontStore spriteFontStore;

			public DrawNode ( SpriteFontDisplay source, int subtreeIndex ) : base( source, subtreeIndex ) {
				font = source.font;
				spriteFontStore = source.spriteFontStore;
			}

			GlyphId pageId;
			protected override void UpdateState () {
				base.UpdateState();
				pageId = new( Source.pageId * 128 );
				page = null;
			}

			SpriteFontPage? page;
			ISampler? sampler;
			public override void Draw ( ICommandBuffer commands ) {
				if ( page == null ) {
					var spriteFont = spriteFontStore.GetSpriteFont( font );
					page = spriteFont.GetPage( pageId );

					sampler ??= commands.Renderer.CreateSampler();
				}

				base.Draw( commands );
			}

			protected override bool UpdateTexture ( [NotNullWhen(true)] out ITexture2DView? texture, [NotNullWhen(true)] out ISampler? sampler ) {
				texture = page!.View;
				sampler = this.sampler!;

				return true;
			}

			public override void ReleaseResources ( bool willBeReused ) {
				sampler?.Dispose();
				sampler = null;
				base.ReleaseResources( willBeReused );
			}
		}
	}
}
