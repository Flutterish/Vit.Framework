using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class FMul : Instruction {
	public FMul ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public uint ResultTypeId;
	public uint ResultId;
	public uint LeftId;
	public uint RightId;

	public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
		var left = scope.VariableInfo[LeftId];
		var right = scope.VariableInfo[RightId];
		var result = scope.VariableInfo[ResultId];

		var count = result.Type.Size / sizeof( float );
		for ( int i = 0; i < count; i++ ) {
			var a = memory.Read<float>( left.Address + sizeof(float) * i );
			var b = memory.Read<float>( right.Address + sizeof(float) * i);
			memory.Write<float>( result.Address + sizeof(float) * i, a * b );
		}
	}
}
