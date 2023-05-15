using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;

namespace Vit.Framework.Tests.Layout;

public class LayoutContainerTest : Container<IDrawable> {
	public LayoutContainerTest () {
		AddChild( new Sprite {
			Size = (1080, 1080)
		} );


		//var image = Image.Load<Rgba32>( "./texture.jpg" );
		//image.Mutate( x => x.Flip( FlipMode.Vertical ) );
		//var sampleTexture = new Texture( image );
		//TextureIdentifier identifier = new() { Name = "./texture.jpg" };
		//textureStore.AddTexture( identifier, sampleTexture );
	}
}
