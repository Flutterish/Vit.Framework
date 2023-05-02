namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeExtract : Instruction {
	public CompositeExtract ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint CompositeId;
	public uint[] Indices = Array.Empty<uint>();

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = ({GetValue(CompositeId)}){string.Join("", Indices.Select( x => $"[{x}]" ))}";
	}
}
