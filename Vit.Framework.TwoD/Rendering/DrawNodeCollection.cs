using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Interop;

namespace Vit.Framework.TwoD.Rendering;

public class DrawNodeCollection {
	List<DrawNode> nodes = new();

	public void Add ( DrawNode node ) {
		nodes.Add( node );
	}

	public void Clear () {
		nodes.Clear();
	}

	public void Draw ( ICommandBuffer commands ) {
		foreach ( var i in nodes.AsSpan() ) {
			i.Draw( commands );
		}
	}
}
