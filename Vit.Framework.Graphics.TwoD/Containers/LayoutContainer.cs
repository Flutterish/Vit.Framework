using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Mathematics.LinearAlgebra;

namespace Vit.Framework.Graphics.TwoD.Containers;

public class LayoutContainer<T> : LayoutContainer<T, LayoutParams> where T : ILayoutElement {
	protected override void PerformLayout () {
		var size = ContentSize;
		var offset = new Vector2<float>( Padding.Left, Padding.Bottom );

		foreach ( var (i, param) in LayoutChildren ) {
			i.Size = param.Size.GetSize( size );

			var origin = param.Origin.GetValue( i.Size );
			var anchor = param.Anchor.GetValue( size );

			i.Position = (anchor - origin + offset).FromOrigin();
		}
	}
}

public struct LayoutParams {
	/// <summary>
	/// Size of the element.
	/// </summary>
	public RelativeSize2<float> Size;
	/// <summary>
	/// Point on the element which will be placed on <see cref="Anchor"/>.
	/// </summary>
	public RelativePoint2<float> Origin;
	/// <summary>
	/// Point on the parent on which <see cref="Origin"/> will be placed.
	/// </summary>
	public RelativePoint2<float> Anchor;
}
