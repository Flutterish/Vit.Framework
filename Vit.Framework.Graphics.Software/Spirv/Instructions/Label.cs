using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Label : Instruction {
	public Label ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint Id;

	public override string ToString () {
		return $"label {Id}:";
	}
}
