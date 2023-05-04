using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class SampledImageType : DataType {
	public SampledImageType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint ImageTypeId;

	public override IRuntimeType GetRuntimeType () {
		return new RuntimeSamplerType( GetDataType( ImageTypeId ).GetRuntimeType() );
	}

	public override string ToString () {
		return $"Sampler of {GetDataType(ImageTypeId)}";
	}
}
