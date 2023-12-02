using System.Collections.Concurrent;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Threading;
using Vit.Framework.Timing;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Templates;

public abstract partial class Basic2DApp {
	protected abstract class UpdateThread : AppThread {
		DrawNodeRenderer drawNodeRenderer;
		RenderThreadScheduler disposeScheduler;
		StopwatchClock clock;
		public readonly ConcurrentQueue<Action> Scheduler = new();
		protected IHasDrawNodes<DrawNode> Root => drawNodeRenderer.Root;
		protected IReadOnlyDependencyCache Dependencies;
		public UpdateThread ( DrawNodeRenderer drawNodeRenderer, RenderThreadScheduler disposeScheduler, IReadOnlyDependencyCache dependencies, string name ) : base( name ) {
			this.drawNodeRenderer = drawNodeRenderer;
			this.disposeScheduler = disposeScheduler;
			Dependencies = dependencies;
			clock = dependencies.Resolve<StopwatchClock>();
		}

		/// <summary>
		/// The renderer for which draw nodes are created.
		/// </summary>
		public IRenderer? Renderer;

		public bool IsUpdatingActive = true;
		protected sealed override void Loop () {
			while ( Scheduler.TryDequeue( out var action ) ) {
				action();
			}

			if ( !IsUpdatingActive )
				return;

			clock.Update();
			OnUpdate();

			if ( Renderer == null )
				return;

			drawNodeRenderer.CollectDrawData( Renderer, disposeScheduler.Swap );
		}

		protected abstract void OnUpdate ();
	}
}
