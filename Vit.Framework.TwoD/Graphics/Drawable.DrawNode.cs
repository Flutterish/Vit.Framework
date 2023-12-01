using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Mathematics.LinearAlgebra;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.Graphics;

public partial class Drawable : IHasDrawNodes<DrawNode> {
	protected DrawNodeInvalidations DrawNodeInvalidations;
	protected void InvalidateDrawNodes () {
		if ( !DrawNodeInvalidations.InvalidateDrawNodes() )
			return;

		Parent?.OnDrawableDrawNodesInvalidated( this );
	}

	public IDrawableParent? Parent;

	DrawNode?[] drawNodes = new DrawNode?[3];
	protected abstract DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation;
	DrawNode getDrawNode<TSpecialisation> ( int subtreeIndex ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		var node = drawNodes[subtreeIndex] ??= CreateDrawNode<TSpecialisation>( subtreeIndex );
		node.Update();
		return node;
	}

	public void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		collection.Add( getDrawNode<TSpecialisation>( subtreeIndex ) );
	}

	public virtual void DisposeDrawNodes () {
		InvalidateDrawNodes();
		for ( int i = 0; i < 3; i++ ) {
			drawNodes[i]?.Dispose();
			drawNodes[i] = null;
		}
	}

	public abstract class DrawableDrawNode<T> : DrawNode where T : Drawable {
		protected readonly T Source;
		protected DrawableDrawNode ( T source, int subtreeIndex ) : base( subtreeIndex ) {
			Source = source;
		}

		public override void Update () {
			if ( Source.DrawNodeInvalidations.ValidateDrawNode( SubtreeIndex ) )
				UpdateState();
		}

		protected Matrix3<float> UnitToGlobalMatrix;
		protected virtual void UpdateState () {
			UnitToGlobalMatrix = Source.unitToGlobalMatrix;
		}
	}
}
