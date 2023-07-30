using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public partial class Drawable : IHasDrawNodes<DrawNode> {
	protected DrawNodeInvalidations DrawNodeInvalidations;
	protected void InvalidateDrawNodes () {
		if ( !DrawNodeInvalidations.InvalidateDrawNodes() )
			return;

		DrawNodesInvalidated?.Invoke();
	}

	public Action? DrawNodesInvalidated;

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode ( int subtreeIndex );
	public DrawNode GetDrawNode ( int subtreeIndex ) {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode( subtreeIndex );
		node.Update();
		return node;
	}

	public virtual void DisposeDrawNodes () {
		foreach ( var node in drawNodes ) {
			node?.Dispose();
		}
	}

	public abstract class DrawableDrawNode<T> : DrawNode where T : Drawable {
		protected readonly T Source;
		protected DrawableDrawNode ( T source, int subtreeIndex ) : base( subtreeIndex ) {
			Source = source;
		}

		public override void Update () {
			if ( Source.DrawNodeInvalidations.ValidateDrawNode( SubtreeIndex ) )
				UpdateState();
		}

		protected Matrix3<float> UnitToGlobalMatrix;
		protected virtual void UpdateState () {
			UnitToGlobalMatrix = Source.unitToGlobalMatrix;
		}
	}
}
