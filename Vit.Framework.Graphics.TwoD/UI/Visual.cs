using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual : Visual<IDrawable> {
	public static implicit operator Visual ( Drawable drawable )
		=> new Visual { Displayed = drawable };
}
public class Visual<T> : UIComponent where T : IDrawable {
	public static implicit operator Visual<T> ( T drawable )
		=> new Visual<T> { Displayed = drawable };
	public required T Displayed { get; init; }

	protected override bool OnMatrixInvalidated () {
		if ( !base.OnMatrixInvalidated() )
			return false;

		InvalidateLayout( LayoutInvalidations.Child );
		return true;
	}

	protected override void PerformLayout () {
		var globalPosition = LocalSpaceToScreenSpace( Point2<float>.Zero );
		var globalSize = LocalSpaceToScreenSpace( new Point2<float>( Width, Height ) ) - globalPosition;

		Displayed.Position = globalPosition;
		Displayed.Scale = new( globalSize.X, globalSize.Y );
	}

	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		Displayed.TryLoad( dependencies );
	}

	protected override void OnDispose () {
		Displayed.Dispose();
	}

	public override Drawable.DrawNode GetDrawNode ( int subtreeIndex ) {
		return Displayed.GetDrawNode( subtreeIndex );
	}
}
