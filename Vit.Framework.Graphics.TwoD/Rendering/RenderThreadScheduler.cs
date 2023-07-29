using Vit.Framework.Graphics.TwoD.Rendering;

namespace Vit.Framework.Graphics.TwoD;

public class RenderThreadScheduler {
	SwapStack<IHasDrawNodes<DrawNode>> drawNodes = new();
	SwapStack<IDisposable> disposables = new();

	public void ScheduleDrawNodeDisposal ( IHasDrawNodes<DrawNode> drawNodeSource ) {
		drawNodes.Push( drawNodeSource );
	}
	public void ScheduleDisposal ( IDisposable disposable ) {
		disposables.Push( disposable );
	}

	public void Swap ( int index ) {
		drawNodes.Swap( index );
		disposables.Swap( index );
	}

	public void Execute ( int index ) {
		foreach ( var i in drawNodes.PopAll( index ) ) {
			i.DisposeDrawNodes();
		}
		foreach ( var i in disposables.PopAll( index ) ) {
			i.Dispose();
		}
	}

	class SwapStack<T> {
		Stack<T> swap = new();
		Stack<T>?[] backlog = new Stack<T>?[3];

		public void Push ( T value ) {
			swap.Push( value );
		}

		public void Swap ( int index ) {
			(swap, backlog[index]) = (backlog[index] ?? new(), swap);
		}

		public IEnumerable<T> PopAll ( int index ) {
			for ( int i = 0; i < backlog.Length; i++ ) {
				if ( i == index || backlog[i] is not Stack<T> stack )
					continue;

				while ( stack.TryPop( out var value ) ) {
					yield return value;
				}
			}
		}
	}
}