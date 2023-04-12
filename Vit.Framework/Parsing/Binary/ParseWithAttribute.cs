using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Parsing.Binary;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ParseWithAttribute : Attribute {
	public string Ref;

	public ParseWithAttribute ( string @ref ) {
		Ref = @ref;
	}

	public object? GetValue ( BinaryFileParser.Context context ) {
		return context.GetRef<object?>( Ref );
	}
}
