using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI;

public class EmptyUIComponent : UIComponent {
	protected override void PerformLayout () { }
	public override void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection ) { }
	public override void DisposeDrawNodes () { }
}

public class EmptyHoverableUIComponent : EmptyUIComponent, IHoverable {
	public bool OnCursorEntered ( CursorEnteredEvent @event ) {
		return true;
	}

	public bool OnCursorExited ( CursorExitedEvent @event ) {
		return true;
	}

	public bool OnHovered ( HoveredEvent @event ) {
		return true;
	}
}