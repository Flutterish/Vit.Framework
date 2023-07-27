namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual<T> : UIComponent where T : IDrawable {
	public T? Displayed { get; set; }
}
