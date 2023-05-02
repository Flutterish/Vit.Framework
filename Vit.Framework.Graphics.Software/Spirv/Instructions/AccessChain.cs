namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class AccessChain : Instruction {
	public AccessChain ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint BaseId;
	public uint[] Indices = Array.Empty<uint>();

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = &({GetValue(BaseId)}){string.Join("", Indices.Select( x => $"[{GetValue(x)}]" ))}";
	}
}
