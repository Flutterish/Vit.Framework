﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Insights.DrawVisualizer;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class Visual : Visual<Drawable> {
	public Visual ( Drawable displayed ) : base( displayed ) { }
}
public class Visual<T> : UIComponent, IViewableInDrawVisualiser where T : Drawable {
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

	public override DrawNode GetDrawNode<TSpecialisation> ( int subtreeIndex ) {
		return Displayed.GetDrawNode<TSpecialisation>( subtreeIndex );
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
