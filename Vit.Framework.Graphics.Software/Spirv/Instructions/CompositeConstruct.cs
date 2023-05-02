using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class CompositeConstruct : Instruction {
	public CompositeConstruct ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint[] Values = Array.Empty<uint>();

	public override string ToString () {
		return $"{GetValue(ResultId)} = <{string.Join(", ", Values.Select( GetValue ))}>";
	}
}
