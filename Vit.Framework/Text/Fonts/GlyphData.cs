using System.Text;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Text.Fonts;

public class GlyphData {
	public Rune Symbol;
	public readonly List<Vector2<float>> Vertices = new();
}
