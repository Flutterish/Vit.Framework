using System.Diagnostics;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeExtract : Instruction {
	public CompositeExtract ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint CompositeId;
	public uint[] Indices = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope ) {
		Debug.Assert( Indices.Length == 1 );

		var index = Indices[0];
		var to = scope.Variables[ResultId];
		var from = (ICompositeVariable)scope.Variables[CompositeId];

		to.Value = from[index].Value;
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = ({GetValue(CompositeId)}){string.Join("", Indices.Select( x => $"[{x}]" ))}";
	}
}
