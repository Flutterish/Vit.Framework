using System.Diagnostics;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class ImageSample : Instruction {
	public ImageSample ( SourceRef sourceRef ) : base( sourceRef, uint.MaxValue ) { }

	public required bool ImplicitLod;
	public uint ResultTypeId;
	public uint ResultId;
	public uint SampledImageId;
	public uint CoordinateId;
	public ImageOperands? Operands;
	public uint[] Ids = Array.Empty<uint>();

	//public override void Execute ( RuntimeScope scope, ShaderMemory memory ) {
	//	Debug.Assert( Ids.Length == 0 );
	//	var sampler = scope.Opaques.Samplers[memory.Read<OpaqueHandle>( scope.VariableInfo[SampledImageId].Address )];
	//	var coordType = scope.VariableInfo[CoordinateId];

	//	var coord = memory.Read<Point2<float>>( coordType.Address );
	//	var uv = new Point2<int>( (int)MathF.Floor(coord.X * sampler.Size.Width), (int)MathF.Floor( coord.Y * sampler.Size.Height ) );
	//	ColorRgba<float> color;
	//	if ( uv.X < 0 || uv.X >= sampler.Size.Width || uv.Y < 0 || uv.Y >= sampler.Size.Height ) {
	//		color = new( 0, 0, 0, 1 );
	//	}
	//	else {
	//		const float conv = 1 / 255f;
	//		var bytes = sampler.Image[uv.X, uv.Y];
	//		color = new() {
	//			R = bytes.R * conv,
	//			G = bytes.G * conv,
	//			B = bytes.B * conv,
	//			A = bytes.A * conv
	//		};
	//	}

	//	memory.Write( scope.VariableInfo[ResultId].Address, value: color );
	//}

	JitedVariable handle;
	JitedVariable coord;
	JitedVariable result;
	protected override void JitCompile ( RuntimeScope scope, int stackPointer ) {
		Debug.Assert( Ids.Length == 0 );

		handle = JitVariable( SampledImageId, scope, stackPointer );
		coord = JitVariable( CoordinateId, scope, stackPointer );
		result = JitVariable( ResultId, scope, stackPointer );
	}

	protected override void ExecuteCompiled ( ShaderOpaques opaques, ShaderMemory memory ) {
		var sampler = opaques.Samplers[memory.Read<OpaqueHandle>( handle.Address( memory.StackPointer ) )];

		var coord = memory.Read<Point2<float>>( this.coord.Address( memory.StackPointer ) );
		var uv = new Point2<int>( (int)MathF.Floor( coord.X * sampler.Size.Width ), (int)MathF.Floor( coord.Y * sampler.Size.Height ) );
		ColorSRgba<float> color;
		if ( uv.X < 0 || uv.X >= sampler.Size.Width || uv.Y < 0 || uv.Y >= sampler.Size.Height ) {
			color = new( 0, 0, 0, 1 );
		}
		else {
			const float conv = 1 / 255f;
			var bytes = sampler.Image[uv.X, uv.Y];
			color = new() {
				R = bytes.R * conv,
				G = bytes.G * conv,
				B = bytes.B * conv,
				A = bytes.A * conv
			};
		}

		memory.Write( result.Address( memory.StackPointer ), value: color );
	}

	public override string ToString () {
		return $"{GetAssignable(ResultId)} = texture({GetValue(SampledImageId)}, {GetValue(CoordinateId)})";
	}
}
