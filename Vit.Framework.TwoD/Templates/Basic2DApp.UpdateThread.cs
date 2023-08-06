using System.Collections.Concurrent;
using Vit.Framework.Threading;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp<TRoot> {
	protected abstract class UpdateThread : AppThread {
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		protected TRoot Root => (TRoot)drawNodeRenderer.Root;
		public UpdateThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, string name ) : base( name ) {
			this.drawNodeRenderer = drawNodeRenderer;
			this.disposeScheduler = disposeScheduler;
		}

		protected sealed override void Loop () {
			while ( Scheduler.TryDequeue( out var action ) ) {
				action();
			}

			OnUpdate();
			drawNodeRenderer.CollectDrawData( disposeScheduler.Swap );
		}

		protected abstract void OnUpdate ();
	}
}
