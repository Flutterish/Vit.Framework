﻿using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual : Visual<Drawable> { }
public class Visual<T> : UIComponent where T : Drawable {
	public static implicit operator Visual<T> ( T source )
		=> new() { Displayed = source };

	T displayed = null!;
	public required T Displayed {
		get => displayed;
		init {
			displayed = value;
			displayed.DrawNodesInvalidated = onDrawNodesInvalidated;
		}
	}

	protected override void OnMatrixInvalidated () {
		InvalidateLayout( LayoutInvalidations.Child );
	}

	void onDrawNodesInvalidated () {
		Parent?.OnChildDrawNodesInvalidated();
	}

	protected override void PerformLayout () { // TODO just copy the matrix (need: simplify drawables)
		var globalPosition = LocalSpaceToScreenSpace( Point2<float>.Zero );
		var globalSize = LocalSpaceToScreenSpace( new Point2<float>( Width, Height ) ) - globalPosition;

		Displayed.Position = globalPosition;
		Displayed.Scale = new( globalSize.X, globalSize.Y );
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		Displayed.TryLoad( dependencies );
	}
	protected override void OnUnload () {
		
	}

	protected override void OnDispose () {
		Displayed.Dispose();
	}

	public override DrawNode GetDrawNode ( int subtreeIndex ) {
		return Displayed.GetDrawNode( subtreeIndex );
	}

	public override void DisposeDrawNodes () { }
}
