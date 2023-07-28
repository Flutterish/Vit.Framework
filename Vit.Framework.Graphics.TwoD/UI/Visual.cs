using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.TwoD.Rendering;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual : Visual<IDrawable> { }
public class Visual<T> : UIComponent where T : IDrawable {
	public required T Displayed { get; init; }

	protected override void OnMatrixInvalidated () {
		InvalidateLayout( LayoutInvalidations.Child );
	}

	protected override void PerformLayout () {
		var globalPosition = LocalSpaceToScreenSpace( Point2<float>.Zero );
		var globalSize = LocalSpaceToScreenSpace( new Point2<float>( Width, Height ) ) - globalPosition;

		Displayed.Position = globalPosition;
		Displayed.Scale = new( globalSize.X, globalSize.Y );

		Parent?.OnChildDrawNodesInvalidated();
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		Displayed.TryLoad( dependencies );
	}

	protected override void OnDispose () {
		Displayed.Dispose();
	}

	public override DrawNode GetDrawNode ( int subtreeIndex ) {
		return Displayed.GetDrawNode( subtreeIndex );
	}
}
