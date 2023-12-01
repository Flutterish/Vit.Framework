using System.Diagnostics;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.Interop;

namespace Vit.Framework.TwoD.Rendering;

public class DrawNodeCollection {
	List<DrawNode> nodes = new();
	List<(int length, DrawNodeBatchContract contract)> batches = new();

	public void Add ( DrawNode node ) {
		nodes.Add( node );
	}

	public void Clear () {
		nodes.Clear();
		batches.Clear();
	}

	public void Compile () {
		Debug.Assert( batches.Count == 0, "Collection was already compiled, or wasnt cleared" );

		if ( nodes.Count == 0 )
			return;

		int length = 1;
		DrawNodeBatchContract contract = nodes[0].BatchContract;

		for ( int i = 1; i < nodes.Count; i++ ) {
			var node = nodes[i];
			var nodeContract = node.BatchContract;

			if ( nodeContract == contract ) {
				length++;
			}
			else {
				batches.Add( (length, contract) );
				contract = nodeContract;
				length = 1;
			}
		}
		batches.Add( (length, contract) );
	}

	public void Draw ( ICommandBuffer commands ) {
		Debug.Assert( batches.Count != 0 || nodes.Count == 0, "Draw node collection was not compiled" );
		var span = nodes.AsSpan();
		foreach ( var (length, contract) in batches ) {
			contract.Draw( commands, span.Slice( 0, length ) );
			span = span.Slice( length );
		}
	}
}
