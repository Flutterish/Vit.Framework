using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Parsing.Binary;

public class TypeSelectorAttribute : Attribute {
	public string Ref;

	public TypeSelectorAttribute ( string @ref ) {
		Ref = @ref;
	}

	public Type? GetValue ( BinaryFileParser.Context context ) {
		return context.GetRef<Type>( Ref );
	}
}
