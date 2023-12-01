using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class Visual : Visual<Drawable> {
	public Visual ( Drawable displayed ) : base( displayed ) { }
}
public class Visual<T> : UIComponent, IViewableInDrawVisualiser, IDrawableParent where T : Drawable {
	public Visual ( T displayed ) {
		Displayed = displayed;
		displayed.Parent = this;
	}

	public readonly T Displayed;

	public void OnDrawableDrawNodesInvalidated ( Drawable drawable ) {
		Parent?.OnChildDrawNodesInvalidated();
	}

	protected override void OnMatrixInvalidated () {
		InvalidateLayout( LayoutInvalidations.Self );
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

	public override void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection ) {
		Displayed.PopulateDrawNodes<TSpecialisation>( subtreeIndex, collection );
	}

	public override void DisposeDrawNodes () {
		Displayed.DisposeDrawNodes();
	}

	IEnumerable<IViewableInDrawVisualiser> IViewableInDrawVisualiser.Children {
		get {
			yield return Displayed;
		}
	}
}
