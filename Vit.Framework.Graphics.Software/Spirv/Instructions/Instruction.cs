using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

[DebuggerDisplay( "{DeuggerDisplay,nq}" )]
public abstract class Instruction : CompilerObject { // TODO in theory we can spcialise these with generics to know variable types in advance
	public SourceRef SourceRef;

	protected Instruction ( SourceRef sourceRef, uint id ) : base( sourceRef.Compiler, id ) {
		SourceRef = sourceRef;
	}

	public bool IsJited { get; protected set; } = false;
	public virtual void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		if ( !IsJited ) {
			JitCompile( scope, memory.StackPointer );
			IsJited = true;
		}

		ExecuteCompiled( scope.Opaques, memory );
	}

	protected virtual void JitCompile ( RuntimeScope scope, int stackPointer ) {
		throw new NotImplementedException();
	}

	protected virtual void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		throw new NotImplementedException();
	}

	protected struct JitedVariable {
		public bool IsGlobal;
		public bool IsConstant;
		public int AddressOrOffset;
		public int Size;
		public SpirvCompiler Compiler;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Address ( int stackPointer ) {
			Debug.Assert( !IsConstant );
			return IsGlobal ? AddressOrOffset : ( AddressOrOffset + stackPointer );
		}

		public void CopyTo ( int address, ShaderMemory memory ) {
			if ( IsConstant ) {
				Compiler.Constants[(uint)AddressOrOffset].DataSpan[..Size].CopyTo( memory.Memory[address..] );
			}
			else {
				memory.Copy( Address( memory.StackPointer ), address, Size );
			}
		}

		public T Read<T> ( ShaderMemory memory ) where T : unmanaged {
			if ( IsConstant ) {
				return MemoryMarshal.Read<T>( Compiler.Constants[(uint)AddressOrOffset].DataSpan );
			}
			else {
				return memory.Read<T>( Address( memory.StackPointer ) );
			}
		}
	}

	protected JitedVariable JitVariable ( uint id, RuntimeScope scope, int stackPointer ) {
		var variable = scope.VariableInfo[id];
		var isGlobal = GetValue( id ) is not Intermediate;
		var isConstant = GetValue( id ) is Constant or ConstantComposite;

		return new() {
			IsGlobal = isGlobal,
			IsConstant = isConstant,
			Size = variable.Type.Size,
			AddressOrOffset = isConstant ? (int)id : isGlobal ? variable.Address : (variable.Address - stackPointer),
			Compiler = Compiler
		};
	}

	protected virtual string DeuggerDisplay => IsJited ? ToString() : $"!Not Jited | {ToString()}";

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