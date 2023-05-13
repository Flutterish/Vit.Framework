using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Function : Instruction {
	public Function ( SourceRef source, uint id ) : base( source, id ) { }

	public uint ReturnTypeId;
	public FunctionControl Control;
	public uint TypeId;

	public readonly Dictionary<uint, Intermediate> Intermediates = new();
	public readonly Dictionary<uint, int> LabelOffsets = new();
	public readonly List<Instruction> Instructions = new();
	public void AddInstruction ( Instruction instruction ) {
		Instructions.Add( instruction );
		if ( instruction is Label label ) {
			LabelOffsets.Add( label.Id, Instructions.Count );
		}
	}

	public override string ToString () {
		var type = (FunctionType)GetDataType( TypeId );
		return $"function{tryPadLeft(GetName(Id))} ({type.ArgsString}) -> {GetDataType(type.ReturnTypeId)}{(Control != FunctionControl.None ? $" : {Control}" : "" )}";
	}

	string? tryPadLeft ( string? str ) {
		return str is null ? str : ( ' ' + str );
	}
}
