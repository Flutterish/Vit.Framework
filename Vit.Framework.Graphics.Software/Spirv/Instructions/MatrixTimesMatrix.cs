using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class MatrixTimesMatrix : Instruction {
	public MatrixTimesMatrix ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint LeftMatrixId;
	public uint RightMatrixId;

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var left = scope.VariableInfo[LeftMatrixId];
		var right = scope.VariableInfo[RightMatrixId];
		var result = scope.VariableInfo[ResultId];

		((IMatrixType)left.Type).MultiplyMatrix( left, right, result, memory );
	}
}
