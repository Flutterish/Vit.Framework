using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Parsing.Binary;

public class ParsingDependsOnAttribute : Attribute {
	public Type[] Dependencies;

	public ParsingDependsOnAttribute ( params Type[] dependencies ) {
		Dependencies = dependencies;
	}
}
