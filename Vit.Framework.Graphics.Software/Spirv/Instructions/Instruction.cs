using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public abstract class Instruction : CompilerObject {
	public SourceRef SourceRef;

	protected Instruction ( SourceRef sourceRef ) : base( sourceRef.Compiler ) {
		SourceRef = sourceRef;
	}

	public override string ToString () {
		return SourceRef.ToString();
	}
}

public struct SourceRef {
	public SpirvCompiler Compiler;
	public OpCode Spirv;
	public uint Line;
	public uint SpirvLine;

	public override string ToString () {
		return $"L{Line} : {Spirv}";
	}
}