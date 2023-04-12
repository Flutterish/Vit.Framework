using System.Reflection;

namespace Vit.Framework.Parsing.Binary;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DataOffsetAttribute : Attribute {
	public string Ref;

	public DataOffsetAttribute ( string @ref ) {
		Ref = @ref;
	}

	MemberInfo? sourceMember;
	public long GetValue ( BinaryFileParser.Context ctx ) {
		sourceMember ??= ctx.MemberValues!.Keys.First( x => x.Name == Ref );
		var data = ctx.MemberValues![sourceMember];

		long value = ctx.Offset;

		if ( data is long i64 )
			value += i64;
		else if ( data!.GetType().GetMethods( BindingFlags.Static | BindingFlags.Public ).FirstOrDefault( x => x.ReturnType == typeof(long) && x.Name == "op_Implicit" ) is MethodInfo method )
			value += (long)method.Invoke( null, new[] { data } )!;
		else
			throw new Exception( "Target member is not a valid offset" );

		return value;
	}
}
