namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual<T> : UIComponent where T : IDrawable {
	public T? Displayed { get; set; }

	protected override bool OnMatrixInvalidated () {
		if ( !base.OnMatrixInvalidated() )
			return false;

		InvalidateLayout( LayoutInvalidations.Child );
		return true;
	}

	protected override void PerformLayout () {
		throw new NotImplementedException(); // TODO set drawable matrix
	}
}
