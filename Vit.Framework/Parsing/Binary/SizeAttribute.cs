namespace Vit.Framework.Parsing.Binary;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SizeAttribute : Attribute {
	public string? Ref;
	public int? Value;
	public double Multiplier;

	public SizeAttribute ( string? @ref, double multiplier = 1 ) {
		Ref = @ref;
		Multiplier = multiplier;
	}

	public SizeAttribute ( int value ) {
		Value = value;
	}

	public int GetValue ( BinaryFileParser.Context context ) {
		if ( Ref == null )
			return Value.GetValueOrDefault();

		var value = context.GetRef<int>( Ref );

		return (int)(value * Multiplier);
	}
}
