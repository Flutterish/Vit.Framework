namespace Vit.Framework.Parsing.Binary;

public class TypeSelectorAttribute : Attribute {
	public string Ref;

	public TypeSelectorAttribute ( string @ref ) {
		Ref = @ref;
	}
}
