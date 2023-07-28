using Vit.Framework.Graphics.TwoD.Rendering;

namespace Vit.Framework.Graphics.TwoD;

public class RenderThreadScheduler {
	Stack<IHasDrawNodes<DrawNode>> swapTree = new();
	Stack<IHasDrawNodes<DrawNode>>?[] disposeTree = new Stack<IHasDrawNodes<DrawNode>>?[3];

	public void ScheduleDisposal ( IHasDrawNodes<DrawNode> drawNodeSource ) {
		swapTree.Push( drawNodeSource );
	}

	public void Swap ( int index ) {
		(swapTree, disposeTree[index]) = (disposeTree[index] ?? new(), swapTree);
	}

	public void Execute ( int index ) {
		for ( int i = 0; i < disposeTree.Length; i++ ) {
			if ( i == index || disposeTree[i] is not Stack<IHasDrawNodes<DrawNode>> stack )
				continue;

			while ( stack.TryPop( out var drawNodeSource ) ) {
				drawNodeSource.DisposeDrawNodes();
			}
		}
	}
}