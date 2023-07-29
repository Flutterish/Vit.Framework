using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public abstract partial class Drawable : DisposableObject, IHasDrawNodes<DrawNode> {
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

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode ( int subtreeIndex );
	public DrawNode GetDrawNode ( int subtreeIndex ) {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode( subtreeIndex );
		node.Update();
		return node;
	}

	protected override void Dispose ( bool disposing ) {
		DrawThreadScheduler.ScheduleDrawNodeDisposal( this );
	}

	public virtual void DisposeDrawNodes () {
		foreach ( var node in drawNodes ) {
			node?.Dispose();
		}
	}
}
