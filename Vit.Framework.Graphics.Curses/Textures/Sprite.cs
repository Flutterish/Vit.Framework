using System.Text;
using Vit.Framework.Graphics.Rendering.Textures;
using Vit.Framework.Mathematics;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.Curses.Textures;

public class Sprite : IFramebuffer {
	Size2<uint> size;
	public Size2<uint> Size {
		get => size;
		set {
			size = value;
			Pixels = new CursesPixel[value.Height, value.Width];
		}
	}

	public CursesPixel[,] Pixels { get; private set; } = new CursesPixel[0, 0];
	public Span2D<CursesPixel> AsSpan () => new Span2D<CursesPixel>( Pixels );

	public override string ToString () {
		var sb = new StringBuilder( (int)(size.Width * 2 + 1) * (int)size.Height );
		for ( int i = 0; i < size.Height; i++ ) {
			foreach ( var px in AsSpan().GetRow( i ) ) {
				sb.Append( px.Symbol );
			}
			sb.Append( '\n' );
		}

		return sb.ToString();
	}

	public void Dispose () { }
}
