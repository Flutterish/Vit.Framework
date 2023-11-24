using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

/// <summary>
/// An object containing state and computations required for drawing a specific thing. It creates up to 3 subtrees of <see cref="DrawNode"/>s for use in a triple-buffer.
/// </summary>
public abstract partial class Drawable {
	public bool IsLoaded { get; private set; }

	public void Load ( IReadOnlyDependencyCache dependencies ) {
		if ( IsLoaded )
			return;

		OnLoad( dependencies );
		IsLoaded = true;
	}
	protected virtual void OnLoad ( IReadOnlyDependencyCache dependencies ) { }

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

	public void Dispose ( RenderThreadScheduler disposeScheduler ) {
		disposeScheduler.ScheduleDrawNodeDisposal( this );
	}
}
