using System.Collections.Concurrent;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Threading;
using Vit.Framework.Timing;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp<TRoot> {
	protected abstract class UpdateThread : AppThread {
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		StopwatchClock clock;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		protected TRoot Root => (TRoot)drawNodeRenderer.Root;
		protected IReadOnlyDependencyCache Dependencies;
		public UpdateThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, IReadOnlyDependencyCache dependencies, string name ) : base( name ) {
			this.drawNodeRenderer = drawNodeRenderer;
			this.disposeScheduler = disposeScheduler;
			Dependencies = dependencies;
			clock = dependencies.Resolve<StopwatchClock>();
		}

		protected sealed override void Loop () {
			while ( Scheduler.TryDequeue( out var action ) ) {
				action();
			}

			clock.Update();
			OnUpdate();
			drawNodeRenderer.CollectDrawData( disposeScheduler.Swap );
		}

		protected abstract void OnUpdate ();
	}
}
