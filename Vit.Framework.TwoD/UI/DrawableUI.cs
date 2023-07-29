using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI;

public class DrawableUI : Drawable {
	RootUIComponent root;
	public DrawableUI ( UIComponent root ) {
		this.root = new( this, root );
	}

	public Size2<float> Size {
		get => root.Child.Size;
		set => root.Child.Size = value;
	}
	public Size2<float> RequiredSize => root.Child.RequiredSize;

	protected override void OnMatrixInvalidated () {
		base.OnMatrixInvalidated();
		root.InvalidateMatrix();
	}

	public override void Update () {
		root.ComputeLayout();
	}

	protected override void Load ( IReadOnlyDependencyCache dependencies ) {
		base.Load( dependencies );
		root.Load( dependencies );
	}

	protected override void Dispose ( bool disposing ) {
		base.Dispose( disposing );
		root.Dispose();
	}

	protected override DrawNode CreateDrawNode ( int subtreeIndex ) {
		return root.GetDrawNode( subtreeIndex );
	}

	class RootUIComponent : CompositeUIComponent<UIComponent> {
		public RootUIComponent ( DrawableUI source, UIComponent child ) {
			Source = source;
			Child = child;

			AddInternalChild( child );
		}

		public DrawableUI Source { get; init; }
		public UIComponent Child { get; init; }

		public void InvalidateMatrix () {
			OnLocalMatrixInvalidated();
		}

		protected override Matrix3<float> ComputeLocalToUnitMatrix () {
			return Source.GlobalToUnitMatrix;
		}
		protected override Matrix3<float> ComputeUnitToLocalMatrix () {
			return Source.UnitToGlobalMatrix;
		}
	}
}
