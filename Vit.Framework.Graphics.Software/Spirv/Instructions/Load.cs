﻿using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Load : Instruction {
	public Load ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint PointerId;
	public MemoryOperands? MemoryOperands;

	public override void Execute ( RuntimeScope scope ) {
		var from = ((PointerVariable)scope.Variables[PointerId]).Address!;
		var to = scope.Variables[ResultId];

		to.Value = from.Value;
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = *({GetValue(PointerId)}){(MemoryOperands == null ? "" : $" {MemoryOperands}")}";
	}
}
