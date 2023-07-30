using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Graphics;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input.Events;

namespace Vit.Framework.TwoD.UI.Graphics;

public class Box : Visual<Sprite>, IEventHandler<HoveredEvent> {
	[SetsRequiredMembers]
	public Box () {
		Displayed = new();
	}

	public ColorRgba<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}

	public bool OnEvent ( HoveredEvent @event ) {
		return true;
	}
}
