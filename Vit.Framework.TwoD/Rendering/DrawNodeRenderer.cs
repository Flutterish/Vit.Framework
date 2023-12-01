using System.Reflection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Rendering.Specialisation;
using Vit.Framework.Threading.Synchronisation;

namespace Vit.Framework.TwoD.Rendering;

public class DrawNodeRenderer {
	TripleBuffer drawNodeSwapchain = new();
	public readonly IHasDrawNodes<DrawNode> Root;

	Dictionary<Type, Action<IHasDrawNodes<DrawNode>, int, DrawNodeCollection>> specialisationCache = new();
	(IRenderer renderer, DrawNodeCollection collection)?[] drawNodes = new (IRenderer, DrawNodeCollection)?[3];
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
		var data = drawNodes[index];
		var collection = data?.collection ?? new();

		populateDrawNodes( renderer, index, collection );
		drawNodes[index] = (renderer, collection);
		action?.Invoke( index );
	}
	void populateDrawNodes ( IRenderer renderer, int index, DrawNodeCollection collection ) {
		collection.Clear();

		var type = renderer.Specialisation.GetType();
		if ( specialisationCache.TryGetValue( type, out var func ) ) {
			func( Root, index, collection );
			return;
		}

		var method = typeof( DrawNodeRenderer ).GetMethod( nameof( populateSpecialisedDrawNode ), BindingFlags.Static | BindingFlags.NonPublic )!;
		var generic = method.MakeGenericMethod( type );

		func = generic.CreateDelegate<Action<IHasDrawNodes<DrawNode>, int, DrawNodeCollection>>();
		specialisationCache[type] = func;

		func( Root, index, collection );
	}

	static void populateSpecialisedDrawNode<TSpecialisation> ( IHasDrawNodes<DrawNode> root, int index, DrawNodeCollection collection ) where TSpecialisation : unmanaged, IRendererSpecialisation {
		root.PopulateDrawNodes<TSpecialisation>( index, collection );
		collection.Compile();
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
		if ( drawNodes[index] is not var (renderer, collection) )
			return;

		if ( commands.Renderer != renderer )
			return;

		collection.Draw( commands );
	}
}
