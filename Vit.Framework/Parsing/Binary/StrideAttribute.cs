namespace Vit.Framework.Parsing.Binary;

public class StrideAttribute : Attribute {
	public string? Ref;
	public int? Value;

	public StrideAttribute ( string? @ref ) {
		Ref = @ref;
	}

	public StrideAttribute ( int value ) {
		Value = value;
	}
}
