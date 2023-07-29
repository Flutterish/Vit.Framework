using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public partial class Drawable {
	protected DrawNodeInvalidations DrawNodeInvalidations;
	protected void InvalidateDrawNodes () {
		if ( !DrawNodeInvalidations.InvalidateDrawNodes() )
			return;

		DrawNodesInvalidated?.Invoke();

		if ( Parent != null ) {
			((Drawable)Parent).InvalidateDrawNodes();
		}
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

		protected abstract void UpdateState ();
	}

	public abstract class BasicDrawNode<T> : DrawableDrawNode<T> where T : Drawable {
		protected Matrix3<float> UnitToGlobalMatrix;
		protected BasicDrawNode ( T source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			UnitToGlobalMatrix = Source.UnitToGlobalMatrix;
		}
	}
}
