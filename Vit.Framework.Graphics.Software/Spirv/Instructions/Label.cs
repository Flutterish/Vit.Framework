﻿using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Label : Instruction {
	public Label ( SourceRef sourceRef, uint id ) : base( sourceRef, id ) { }

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) { }

	protected override string DeuggerDisplay => ToString();

	public override string ToString () {
		return $"label {Id}:";
	}
}
