using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class VectorShuffle : Instruction {
	public VectorShuffle ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint VectorAId;
	public uint VectorBId;
	public uint[] Components = Array.Empty<uint>();

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var vectorA = scope.VariableInfo[VectorAId];
		var vectorB = scope.VariableInfo[VectorBId];
		var result = scope.VariableInfo[ResultId];

		var aLength = ((IRuntimeVectorType)vectorA.Type).Length;
		var componentType = ((IRuntimeVectorType)vectorA.Type).ComponentType;

		for ( int i = 0; i < Components.Length; i++ ) {
			var index = Components[i];
			var sourceOffset = index < aLength ? (vectorA.Address + componentType.Size * index) : (vectorB.Address + componentType.Size * (index - aLength));
			var resultOffset = result.Address + i * componentType.Size;

			memory.GetMemory( (int)sourceOffset, componentType.Size ).CopyTo( memory.Memory[resultOffset..] );
		}
	}

	public override string ToString () {
		var aLength = ( (VectorType)GetValue( VectorAId ).Type ).Count;
		return $"{GetAssignable(ResultId)} = <{string.Join(", ", Components.Select( x => {
			const string names = "xyzw";
			return $"{(x < aLength ? GetValue(VectorAId) : GetValue( VectorBId ) )}.{names[(int)(x < aLength ? x : (x - aLength))]}";
		} ))}>";
	}
}
