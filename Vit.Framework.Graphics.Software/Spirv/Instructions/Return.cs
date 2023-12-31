﻿using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Return : Instruction {
	public Return ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		scope.CodePointer = int.MaxValue;
	}

	protected override string DeuggerDisplay => ToString();

	public override string ToString () {
		return "return";
	}
}
