using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class ImageType : DataType {
	public ImageType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint SampledTypeId;
	public uint Dimensionality;
	public uint DepthType;
	public uint Arrayed;
	public uint IsMultiSampled;
	public uint SamplingType;
	public ImageFormat ImageFormat;
	public AccessQualifier? AccessQualifier;

	public override IRuntimeType GetRuntimeType () {
		return new RuntimeImageType( GetDataType( SampledTypeId ).GetRuntimeType() );
	}

	public override string ToString () {
		return $"Image<{GetDataType(SampledTypeId)}> ({ImageFormat})";
	}
}
