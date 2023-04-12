namespace Vit.Framework.Parsing.Binary;

public class ParsingDependsOnAttribute : Attribute {
	public Type[] Dependencies;

	public ParsingDependsOnAttribute ( params Type[] dependencies ) {
		Dependencies = dependencies;
	}
}
