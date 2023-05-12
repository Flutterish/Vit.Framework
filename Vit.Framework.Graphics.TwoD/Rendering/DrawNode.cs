using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.Memory;

namespace Vit.Framework.Graphics.TwoD;

public partial class Drawable {
	protected void InvalidateDrawNodes () {
		if ( drawNodeInvalidations == 0b_111 )
			return;
		
		drawNodeInvalidations = 0b_111;
		if ( Parent != null ) {
			((Drawable)Parent).InvalidateDrawNodes();
		}
	}
	private byte drawNodeInvalidations = 0b_111;

	public abstract class DrawNode : DisposableObject {
		protected readonly Drawable Source;
		protected readonly int SubtreeIndex;
		protected DrawNode ( Drawable source, int subtreeIndex ) {
			Source = source;
			SubtreeIndex = subtreeIndex;
		}

		/// <summary>
		/// [Update Thread] <br/>
		/// Sets the parameters required to draw this draw node if it is invalidated.
		/// </summary>
		public void Update () {
			if ( (Source.drawNodeInvalidations & (1 << SubtreeIndex)) == 0 )
				return;

			Source.drawNodeInvalidations &= (byte)(~(1 << SubtreeIndex));
			UpdateState();
		}

		/// <summary>
		/// [Update Thread] <br/>
		/// Sets the parameters required to draw this draw node. They <strong>*can not*</strong> be mutated outside this method.
		/// </summary>
		protected abstract void UpdateState ();

		/// <summary>
		/// [Draw Thread] <br/>
		/// Creates required resources and draws the element represented by this draw node.
		/// </summary>
		public abstract void Draw ( ICommandBuffer commands );

		/// <summary>
		/// [Draw Thread] <br/>
		/// Releases any created resources. This might be called because:
		/// <list type="number">
		///		<item>The draw node is being disposed.</item>
		///		<item>The renderer will be changed.</item>
		///		<item>The draw node is in an incative branch of the draw tree.</item>
		/// </list>
		/// </summary>
		/// <param name="willBeReused">Whether this draw node will be used again. If <see langword="false"/>, the draw node is allowed to enter invalid state.</param>
		public abstract void ReleaseResources ( bool willBeReused );
		protected sealed override void Dispose ( bool disposing ) {
			ReleaseResources( willBeReused: false );
		}
	}

	public abstract class BasicDrawNode<T> : DrawNode where T : Drawable {
		new protected T Source => (T)base.Source;
		protected Matrix3<float> UnitToGlobalMatrix;
		protected BasicDrawNode ( T source, int subtreeIndex ) : base( source, subtreeIndex ) { }

		protected override void UpdateState () {
			UnitToGlobalMatrix = Source.UnitToGlobalMatrix;
		}
	}
}