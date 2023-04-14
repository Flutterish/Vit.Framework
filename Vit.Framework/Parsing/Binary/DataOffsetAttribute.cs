namespace Vit.Framework.Parsing.Binary;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DataOffsetAttribute : Attribute {
	public string Ref;

	public DataOffsetAttribute ( string @ref ) {
		Ref = @ref;
	}

	public long GetValue ( BinaryFileParser.Context context ) {
		return context.Offset + context.GetRef<long>( Ref );
	}
}
