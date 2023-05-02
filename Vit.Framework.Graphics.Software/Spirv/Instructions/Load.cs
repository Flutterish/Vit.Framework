using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Load : Instruction {
	public Load ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint PointerId;
	public MemoryOperands? MemoryOperands;

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = *({GetValue(PointerId)}){(MemoryOperands == null ? "" : $" {MemoryOperands}")}";
	}
}
