using System.Text;

namespace Vit.Framework.Graphics.Curses.Textures;

public struct CursesPixel {
	public Rune Symbol;
	public ColorRgba<byte> Foreground;
	public ColorRgba<byte> Background;
}
