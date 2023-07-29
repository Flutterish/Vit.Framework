using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public partial class Drawable {
	protected DrawNodeInvalidations DrawNodeInvalidations;
	protected void InvalidateDrawNodes () {
		if ( !DrawNodeInvalidations.InvalidateDrawNodes() )
			return;

		DrawNodesInvalidated?.Invoke();
	}

	public Action? DrawNodesInvalidated;

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
