using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input.Events;
using Vit.Framework.TwoD.UI.Animations;

namespace Vit.Framework.TwoD.UI.Graphics;

public class Box : Visual<Sprite>, IEventHandler<HoveredEvent>, IHasAlphaTint {
	public Box () : base( new() ) { }

	public ColorRgba<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}

	public Texture Texture {
		get => Displayed.Texture;
		set => Displayed.Texture = value;
	}

	public bool OnEvent ( HoveredEvent @event ) {
		return true;
	}
}
