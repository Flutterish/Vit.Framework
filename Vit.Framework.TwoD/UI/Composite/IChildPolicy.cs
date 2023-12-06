using System.Runtime.CompilerServices;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Input.Events;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;

namespace Vit.Framework.TwoD.UI.Composite;

public interface IChildPolicy<T> where T : UIComponent {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static virtual void UpdateMasking ( IReadOnlyList<T> children, AxisAlignedBox2<float> maskingBounds ) {
		var count = children.Count;
		for ( int i = 0; i < count; i++ ) {
			children[i].UpdateMasking( maskingBounds );
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static virtual void UpdateSubtree ( IReadOnlyList<T> children ) {
		var count = children.Count;
		for ( int i = 0; i < count; i++ ) {
			children[i].Update();
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static virtual void PopulateDrawNodes<TSpecialisation> ( int subtreeIndex, DrawNodeCollection collection, IReadOnlyList<T> children ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		var count = children.Count;
		for ( int i = 0; i < count; i++ ) {
			var child = children[i];
			if ( child.IsVisible )
				child.PopulateDrawNodes<TSpecialisation>( subtreeIndex, collection );
		}
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	static virtual void SortEventTree ( EventTree<UIComponent> tree ) {
		tree.Sort( static ( a, b ) => b.Source.Depth - a.Source.Depth );
	}

	static virtual bool LoadChildren { 
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get => true;
	}

	static virtual bool ProcessChildEvents { 
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get => true; 
	}

	static virtual bool InvalidateChildMatrices { 
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get => true; 
	}
}

public struct DefaultChildPolicy<T> : IChildPolicy<T> where T : UIComponent { }
