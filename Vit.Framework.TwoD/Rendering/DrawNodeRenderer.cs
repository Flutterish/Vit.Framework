using System.Reflection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Threading.Synchronisation;

namespace Vit.Framework.TwoD.Rendering;

public class DrawNodeRenderer {
	TripleBuffer drawNodeSwapchain = new();
	public readonly IHasDrawNodes<DrawNode> Root;

	Dictionary<Type, Func<IHasDrawNodes<DrawNode>, int, DrawNode>> specialisationCache = new();
	(IRenderer renderer, DrawNode node)?[] drawNodes = new (IRenderer, DrawNode)?[3];
	public DrawNodeRenderer ( IHasDrawNodes<DrawNode> root ) {
		Root = root;
	}

	/// <summary>
	/// Collects draw data.
	/// </summary>
	/// <param name="renderer">The renderer that will be used to render the generated draw nodes. This is used to allow draw node specialisation.</param>
	/// <param name="action">An action to perform using the subtree index after collecting draw data.</param>
	public void CollectDrawData ( IRenderer renderer, Action<int>? action = null ) {
		using var _ = drawNodeSwapchain.GetForWrite( out var index );
		if ( Root.IsDisposed ) {
			drawNodes[index] = null;
		}
		else {
			drawNodes[index] = (renderer, getDrawNode( renderer, index ));
		}
		action?.Invoke( index );
	}
	DrawNode getDrawNode ( IRenderer renderer, int index ) {
		var type = renderer.GetType();
		if ( specialisationCache.TryGetValue( type, out var func ) ) {
			return func( Root, index );
		}

		var method = typeof( DrawNodeRenderer ).GetMethod( nameof( getSpecialisedDrawNode ), BindingFlags.Static | BindingFlags.NonPublic )!;
		var generic = method.MakeGenericMethod( type );

		func = generic.CreateDelegate<Func<IHasDrawNodes<DrawNode>, int, DrawNode>>();
		specialisationCache[type] = func;

		return func( Root, index );
	}

	static DrawNode getSpecialisedDrawNode<TRenderer> ( IHasDrawNodes<DrawNode> root, int index ) where TRenderer :IRenderer {
		return root.GetDrawNode<TRenderer>( index );
	}

	public void Draw ( ICommandBuffer commands, Action<int>? action = null ) {
		using var _ = drawNodeSwapchain.GetForRead( out var index, out var _ );
		action?.Invoke( index );
		draw( index, commands );
	}
	public bool DrawIfNew ( ICommandBuffer commands, Action<int>? action = null ) {
		if ( !drawNodeSwapchain.TryGetForRead( out var index, out var dispose ) )
			return false;

		using var _ = dispose;
		action?.Invoke( index );
		draw( index, commands );
		return true;
	}

	void draw ( int index, ICommandBuffer commands ) {
		if ( drawNodes[index] is not var (renderer, drawNode) )
			return;

		if ( commands.Renderer != renderer )
			return;

		drawNode.Draw( commands );
	}
}
