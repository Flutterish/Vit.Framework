using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public partial class Drawable {
	private ulong invalidationId = 1;

	public abstract class DrawNode : DisposableObject {
		protected readonly Drawable Source;
		private ulong invalidationId = 0;
		protected DrawNode ( Drawable source ) {
			Source = source;
		}

		public void Update () {
			if ( Source.invalidationId == invalidationId )
				return;

			invalidationId = Source.invalidationId;
			UpdateState();
		}
		protected abstract void UpdateState ();
		public abstract void Draw ( ICommandBuffer commands );
	}
}