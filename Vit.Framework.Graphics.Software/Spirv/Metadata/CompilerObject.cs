using Vit.Framework.Graphics.Software.Spirv.Instructions;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public abstract class CompilerObject {
	public readonly SpirvCompiler Compiler;
	public readonly uint Id;
	protected CompilerObject ( SpirvCompiler compiler, uint id ) {
		Compiler = compiler;
		Id = id;
	}

	static protected IReadOnlyDictionary<DecorationName, Decoration> emptyDecorations = new Dictionary<DecorationName, Decoration>();
	public IReadOnlyDictionary<DecorationName, Decoration> Decorations => Compiler.Decorations.TryGetValue( Id, out var decorations )
		? decorations
		: emptyDecorations;

	public Constant GetConstant ( uint id ) {
		return Compiler.Constants[id];
	}

	public IValue GetValue ( uint id ) {
		return Compiler.Values[id];
	}

	public IAssignable GetAssignable ( uint id ) {
		return Compiler.Assignables[id];
	}

	public EntryPoint GetEntryPoint ( uint id ) {
		return Compiler.EntryPointsById[id];
	}

	public DataType GetDataType ( uint id ) {
		return Compiler.DataTypes[id];
	}

	public Function GetFunction ( uint id ) {
		return Compiler.Functions[id];
	}

	public Variable GetVariable ( uint id ) {
		return Compiler.Variables[id];
	}

	public string? GetName ( uint id ) {
		return Compiler.Names.GetValueOrDefault( id );
	}

	public string? GetMemberName ( uint id, uint index ) {
		return Compiler.MemberNames.GetValueOrDefault( (id, index) );
	}
}
