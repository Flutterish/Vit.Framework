using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class VoidType : DataType {
	public VoidType ( SpirvCompiler compiler ) : base( compiler ) { }

	public override string ToString () {
		return "void";
	}
}
