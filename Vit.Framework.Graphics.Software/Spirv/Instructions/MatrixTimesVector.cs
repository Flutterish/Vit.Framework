using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class MatrixTimesVector : Instruction {
	public MatrixTimesVector ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint MatrixId;
	public uint VectorId;

	public override void Execute ( RuntimeScope scope ) {
		var matrix = scope.Variables[MatrixId];
		var vector = scope.Variables[VectorId];
		var result = scope.Variables[ResultId];

		matrix.MultiplyVector( vector, result );
	}

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var matrix = scope.VariableInfo[MatrixId];
		var vector = scope.VariableInfo[VectorId];
		var result = scope.VariableInfo[ResultId];

		((IMatrixType)matrix.Type).Multiply( matrix, vector, result, memory );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = {GetValue(MatrixId)} * {GetValue(VectorId)}";
	}
}
