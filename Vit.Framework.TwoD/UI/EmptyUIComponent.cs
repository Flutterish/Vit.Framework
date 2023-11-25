using Vit.Framework.Graphics.Rendering;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class EmptyUIComponent : UIComponent {
	protected override void PerformLayout () { }

	public override DrawNode GetDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return emptyDrawNode;
	}

	public override void DisposeDrawNodes () { }

	static EmptyDrawNode emptyDrawNode = new( -1 );
	public class EmptyDrawNode : DrawNode {
		public EmptyDrawNode ( int subtreeIndex ) : base( subtreeIndex ) { }

		public override void Update () { }

		public override void Draw ( ICommandBuffer commands ) { }

		public override void ReleaseResources ( bool willBeReused ) { }
	}
}
