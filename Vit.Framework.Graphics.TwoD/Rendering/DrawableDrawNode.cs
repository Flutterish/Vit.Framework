using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD;

public partial class Drawable {
	protected void InvalidateDrawNodes () {
		if ( drawNodeInvalidations == 0b_111 )
			return;
		
		drawNodeInvalidations = 0b_111;
		if ( Parent != null ) {
			((Drawable)Parent).InvalidateDrawNodes();
		}
	}
	private byte drawNodeInvalidations = 0b_111;

	public abstract class DrawableDrawNode<T> : DrawNode where T : Drawable {
		protected readonly T Source;
		protected DrawableDrawNode ( T source, int subtreeIndex ) : base( subtreeIndex ) {
			Source = source;
		}

		public override void Update () {
			if ( (Source.drawNodeInvalidations & (1 << SubtreeIndex)) == 0 )
				return;

			Source.drawNodeInvalidations &= (byte)(~(1 << SubtreeIndex));
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

	public class RenderThreadScheduler {
		Stack<Drawable> swapTree = new();
		Stack<Drawable>?[] disposeTree = new Stack<Drawable>?[3];

		public void ScheduleDisposal ( Drawable drawable ) {
			swapTree.Push( drawable );
		}

		public void Swap ( int index ) {
			(swapTree, disposeTree[index]) = (disposeTree[index] ?? new(), swapTree);
		}

		public void Execute ( int index ) {
			for ( int i = 0; i < disposeTree.Length; i++ ) {
				if ( i == index || disposeTree[i] is not Stack<Drawable> stack )
					continue;

				while ( stack.TryPop( out var drawable ) ) {
					foreach ( var node in drawable.drawNodes ) {
						node?.Dispose();
					}
				}
			}
		}
	}
}