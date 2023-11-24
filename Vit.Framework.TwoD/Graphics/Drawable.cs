using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

/// <summary>
/// An object containing state and computations required for drawing a specific thing. It creates up to 3 subtrees of <see cref="DrawNode"/>s for use in a triple-buffer.
/// </summary>
public abstract partial class Drawable : IDisposable {
	public bool IsLoaded { get; private set; }

	protected RenderThreadScheduler DrawThreadScheduler { get; private set; } = null!;
	public void Load ( IReadOnlyDependencyCache dependencies ) {
		if ( IsLoaded )
			return;

		OnLoad( dependencies );
		IsLoaded = true;
	}
	protected virtual void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		DrawThreadScheduler = dependencies.Resolve<RenderThreadScheduler>();
	}

	public void Unload () {
		if ( !IsLoaded )
			return;

		IsLoaded = false;
		OnUnload();
	}
	protected virtual void OnUnload () { }

	Matrix3<float> unitToGlobalMatrix = Matrix3<float>.Identity;
	public Matrix3<float> UnitToGlobalMatrix {
		get => unitToGlobalMatrix;
		set {
			unitToGlobalMatrix = value;
			InvalidateDrawNodes();
		}
	}

	public bool IsDisposed { get; private set; }
	public virtual void Dispose () {
		DrawThreadScheduler.ScheduleDrawNodeDisposal( this );
		IsDisposed = true;
	}
}
