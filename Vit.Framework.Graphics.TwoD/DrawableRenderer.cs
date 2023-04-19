using Vit.Framework.Threading.Synchronisation;

namespace Vit.Framework.Graphics.TwoD;

public class DrawableRenderer {
	TripleBuffer drawNodeSwapchain = new();
	public readonly Drawable Root;

	Drawable.DrawNode[] drawNodes = new Drawable.DrawNode[3];
	public DrawableRenderer ( Drawable root ) {
		Root = root;
	}

	public void CollectDrawData () {
		using var _ = drawNodeSwapchain.GetForWrite( out var index );
		drawNodes[index] = Root.GetDrawNode( index );
	}

	public void Draw () {
		using var _ = drawNodeSwapchain.GetForRead( out var index, out var _ );
		draw( index );
	}
	public bool DrawIfNew () {
		if ( !drawNodeSwapchain.TryGetForRead( out var index, out var dispose ) )
			return false;

		using var _ = dispose;
		draw( index );
		return true;
	}

	void draw ( int index ) {
		drawNodes[index].Draw();
	}
}
