using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Return : Instruction {
	public Return ( SourceRef sourceRef ) : base( sourceRef ) { }

	public override string ToString () {
		return "return";
	}
}
