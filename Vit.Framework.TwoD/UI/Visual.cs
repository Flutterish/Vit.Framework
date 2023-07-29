using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Graphics.Text;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class Visual : Visual<Drawable> {
	public static implicit operator Visual ( Drawable source )
		=> new() { Displayed = source };
}
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
		InvalidateLayout( LayoutInvalidations.Self );
	}

	void onDrawNodesInvalidated () {
		Parent?.OnChildDrawNodesInvalidated();
	}

	protected override void PerformLayout () { // TODO just copy the matrix (need: simplify drawables)
		var globalPosition = LocalSpaceToScreenSpace( Point2<float>.Zero );

		Displayed.Position = globalPosition;
		if ( displayed is SpriteText le ) {
			var globalOne = LocalSpaceToScreenSpace( Point2<float>.One ) - globalPosition;
			le.Scale = new( globalOne.X, globalOne.Y );
		}
		else {
			var globalSize = LocalSpaceToScreenSpace( new Point2<float>( Width, Height ) ) - globalPosition;
			Displayed.Scale = new( globalSize.X, globalSize.Y );
		}
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
