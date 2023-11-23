using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;

namespace Vit.Framework.TwoD.UI.Animations;

public interface IHasPremultipliedTint : IHasAlpha, IHasTint { }

public static class IHasPremultipliedTintExtensions {
	public static ColorRgba<float> GetPremultipliedTint<T> ( this T self ) where T : IHasTint, IHasAlpha {
		return self.Tint.WithOpacity( self.Alpha );
	}
}

public interface IHasTint {
	ColorRgb<float> Tint { get; set; }

	public static readonly AnimationDomain AnimationDomain = new() { Name = "Tint" };
}

public interface IHasAlpha {
	float Alpha { get; set; }

	public static readonly AnimationDomain AnimationDomain = new() { Name = "Alpha" };
}