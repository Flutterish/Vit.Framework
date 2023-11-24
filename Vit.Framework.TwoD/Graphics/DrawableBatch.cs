//using Vit.Framework.DependencyInjection;
//using Vit.Framework.Graphics.Rendering.Specialisation;
//using Vit.Framework.TwoD.Rendering;

//namespace Vit.Framework.TwoD.Graphics;

//public class DrawableBatch<T> : Drawable, IHasCompositeDrawNodes<DrawNode> where T : Drawable {
//	readonly List<T> internalChildren = new();
//	public IReadOnlyList<T> Children => internalChildren;

//	public void AddChildren ( IEnumerable<T> children ) {
//		foreach ( var i in children )
//			AddChild( i );
//	}
//	public void AddChildren ( params T[] children ) {
//		foreach ( var i in children )
//			AddChild( i );
//	}
//	public void AddChild ( T child ) {
//		internalChildren.Add( child );
//		if ( IsLoaded )
//			child.Load( Dependencies );
//		InvalidateDrawNodes();
//	}

//	public void InsertChild ( T child, int index ) {
//		internalChildren.Insert( index, child );
//		if ( IsLoaded )
//			child.Load( Dependencies );
//		InvalidateDrawNodes();
//	}

//	public void RemoveChildren ( IEnumerable<T> children ) {
//		foreach ( var i in children )
//			RemoveChild( i );
//	}
//	public void RemoveChildren ( params T[] children ) {
//		foreach ( var i in children )
//			RemoveChild( i );
//	}
//	public bool RemoveChild ( T child ) {
//		internalChildren.Remove( child );
//		InvalidateDrawNodes();
//		return true;
//	}
//	public void RemoveChildAt ( int index ) {
//		internalChildren.RemoveAt( index );
//		InvalidateDrawNodes();
//	}

//	public void ClearChildren () {
//		while ( internalChildren.Count != 0 ) {
//			internalChildren.RemoveAt( internalChildren.Count - 1 );
//		}

//		InvalidateDrawNodes();
//	}

//	public void DisposeChildren () {
//		while ( internalChildren.Count != 0 ) {
//			var child = internalChildren[^1];
//			internalChildren.RemoveAt( internalChildren.Count - 1 );
//			child.Dispose();
//		}

//		InvalidateDrawNodes();
//	}

//	public IReadOnlyDependencyCache Dependencies { get; private set; } = null!;
//	protected virtual IReadOnlyDependencyCache CreateDependencies ( IReadOnlyDependencyCache parentDependencies ) => parentDependencies;
//	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
//		base.OnLoad( dependencies );

//		Dependencies = CreateDependencies( dependencies );
//		foreach ( var i in internalChildren ) {
//			i.Load( Dependencies );
//		}
//	}

//	protected override void Dispose ( bool disposing ) {
//		base.Dispose( disposing );
//		foreach ( var i in Children ) {
//			i.Dispose();
//		}
//	}

//	public IReadOnlyList<IHasDrawNodes<Rendering.DrawNode>> CompositeDrawNodeSources => internalChildren;
//	protected override DrawNode CreateDrawNode<TSpecialisation> ( int subtreeIndex ) {
//		return new DrawNode<TSpecialisation>( this, subtreeIndex );
//	}

//	public class DrawNode<TSpecialisation> : CompositeDrawNode<DrawableBatch<T>, Rendering.DrawNode, TSpecialisation> where TSpecialisation : unmanaged, IRendererSpecialisation {
//		public DrawNode ( DrawableBatch<T> source, int subtreeIndex ) : base( source, subtreeIndex ) { }

//		protected override bool ValidateChildList () {
//			return Source.DrawNodeInvalidations.ValidateDrawNode( SubtreeIndex );
//		}
//	}
//}