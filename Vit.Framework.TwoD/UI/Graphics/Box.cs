using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.UI.Animations;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI.Graphics;

public class Box : Visual<Sprite>, IHandlesPositionalInput, IHasPremultipliedTint {
	public Box () : base( new() ) { }

	public ColorRgb<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}
	public float Alpha {
		get => Displayed.Alpha;
		set => Displayed.Alpha = value;
	}

	public Texture Texture {
		get => Displayed.Texture;
		set => Displayed.Texture = value;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}
}
