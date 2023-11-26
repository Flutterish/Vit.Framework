using Vit.Framework.Graphics.Rendering;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI.Input.Events;

namespace Vit.Framework.TwoD.UI;

public class EmptyUIComponent : UIComponent {
	protected override void PerformLayout () { }

	public override DrawNode GetDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return emptyDrawNode;
	}

	public override void DisposeDrawNodes () { }

	static EmptyDrawNode emptyDrawNode = new( -1 );
	public class EmptyDrawNode : DrawNode {
		public EmptyDrawNode ( int subtreeIndex ) : base( subtreeIndex ) {
			Dispose();
		}

		public override void Update () { }

		public override void Draw ( ICommandBuffer commands ) { }

		public override void ReleaseResources ( bool willBeReused ) { }
	}
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