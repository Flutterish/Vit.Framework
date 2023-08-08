using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;

namespace Vit.Framework.TwoD.UI.Animations;

public interface IHasAlphaTint : IHasAlpha, IHasTint {
	new ColorRgba<float> Tint { get; set; }
	ColorRgb<float> IHasTint.Tint { get => Tint.Rgb; set => Tint = Tint with { R = value.R, G = value.G, B = value.B }; }
	float IHasAlpha.Alpha { get => Tint.A; set => Tint = Tint with { A = value }; }
}

public interface IHasTint {
	ColorRgb<float> Tint { get; set; }

	public static readonly AnimationDomain AnimationDomain = new() { Name = "Tint" };
}

public interface IHasAlpha {
	float Alpha { get; set; }

	public static readonly AnimationDomain AnimationDomain = new() { Name = "Alpha" };
}