namespace Vit.Framework.Parsing.Binary;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ParseWithAttribute : Attribute {
	public string Ref;

	public ParseWithAttribute ( string @ref ) {
		Ref = @ref;
	}
}
