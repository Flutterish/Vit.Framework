namespace Vit.Framework.Graphics.TwoD;

public partial class Drawable {
	public abstract class DrawNode {
		public void Update () {
			UpdateState();
		}
		protected abstract void UpdateState ();
		public abstract void Draw ();
	}
}