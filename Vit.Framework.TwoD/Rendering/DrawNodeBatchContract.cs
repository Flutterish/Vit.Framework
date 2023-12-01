using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Rendering;

namespace Vit.Framework.TwoD.Rendering;

/// <summary>
/// A contract between draw nodes which allows to draw multiple of them at once.
/// </summary>
public abstract class DrawNodeBatchContract {
	public abstract void Draw ( ICommandBuffer commands, ReadOnlySpan<DrawNode> drawNodes );
}

public abstract class DrawNodeBatchContract<TNode> : DrawNodeBatchContract {
	public sealed override void Draw ( ICommandBuffer commands, ReadOnlySpan<DrawNode> drawNodes ) {
		ref DrawNode first = ref MemoryMarshal.GetReference( drawNodes );
		Draw( commands, MemoryMarshal.CreateReadOnlySpan( ref Unsafe.As<DrawNode, TNode>( ref first ), drawNodes.Length ) );
	}

	public abstract void Draw ( ICommandBuffer commands, ReadOnlySpan<TNode> drawNodes );
}

public sealed class NullDrawNodeBatchContract : DrawNodeBatchContract {
	public static readonly NullDrawNodeBatchContract Instance = new();

	public override void Draw ( ICommandBuffer commands, ReadOnlySpan<DrawNode> drawNodes ) {
		foreach ( var i in drawNodes ) {
			i.Draw( commands );
		}
	}
}