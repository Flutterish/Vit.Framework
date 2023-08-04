﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class Visual : Visual<Drawable> { }
public class Visual<T> : UIComponent where T : Drawable {
	T displayed = null!;
	public required T Displayed {
		get => displayed;
		init {
			displayed = value;
			displayed.DrawNodesInvalidated = onDrawNodesInvalidated;
		}
	}

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