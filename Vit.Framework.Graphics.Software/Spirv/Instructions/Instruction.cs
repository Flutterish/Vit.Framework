using System.Diagnostics;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public abstract class Instruction : CompilerObject { // TODO in theory we can spcialise these with generics to know variable types in advance
	public SourceRef SourceRef;

	protected Instruction ( SourceRef sourceRef, uint id ) : base( sourceRef.Compiler, id ) {
		SourceRef = sourceRef;
	}

	public virtual void Execute ( RuntimeScope scope ) {
		Debug.Fail( "oops" );
	}

	public virtual void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		Debug.Fail( "oops" );
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