using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class Visual : Visual<Drawable> {
	public Visual ( Drawable displayed ) : base( displayed ) { }
}
public class Visual<T> : UIComponent, IHasAnimationTimeline where T : Drawable {
	public Visual ( T displayed ) {
		Displayed = displayed;
		displayed.DrawNodesInvalidated = onDrawNodesInvalidated;
	}

	public readonly T Displayed;

	protected override void OnMatrixInvalidated () {
		InvalidateLayout( LayoutInvalidations.Self );
	}

	void onDrawNodesInvalidated () {
		Parent?.OnChildDrawNodesInvalidated();
	}

	protected override void PerformLayout () {
		Displayed.UnitToGlobalMatrix = Matrix3<float>.CreateScale( Width, Height ) * UnitToGlobalMatrix;
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		Displayed.Load( dependencies );
	}
	protected override void OnUnload () {
		Displayed.Unload();
	}

	protected override void OnDispose () {
		Displayed.Dispose();
	}

	public override DrawNode GetDrawNode ( int subtreeIndex ) {
		return Displayed.GetDrawNode( subtreeIndex );
	}

	public override void DisposeDrawNodes () { }
}
