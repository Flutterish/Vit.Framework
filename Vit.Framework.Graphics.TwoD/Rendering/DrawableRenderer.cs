﻿using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Graphics.Shaders;
using Vit.Framework.Threading.Synchronisation;

namespace Vit.Framework.Graphics.TwoD.Rendering;

public class DrawableRenderer {
	TripleBuffer drawNodeSwapchain = new();
	public readonly Drawable Root;

	Drawable.DrawNode[] drawNodes = new Drawable.DrawNode[3];
	public DrawableRenderer ( Drawable root ) {
		Root = root;
	}

	public void CollectDrawData () {
		using var _ = drawNodeSwapchain.GetForWrite( out var index );
		drawNodes[index] = Root.GetDrawNode( index );
	}

	public void Draw ( ICommandBuffer commands ) {
		using var _ = drawNodeSwapchain.GetForRead( out var index, out var _ );
		draw( index, commands );
	}
	public bool DrawIfNew ( ICommandBuffer commands ) {
		if ( !drawNodeSwapchain.TryGetForRead( out var index, out var dispose ) )
			return false;

		using var _ = dispose;
		draw( index, commands );
		return true;
	}

	void draw ( int index, ICommandBuffer commands ) {
		drawNodes[index].Draw( commands );
	}

	public static readonly ShaderIdentifier TestVertex = new() { Name = "Vertex" };
	public static readonly ShaderIdentifier TestFragment = new() { Name = "Fragment" };
}