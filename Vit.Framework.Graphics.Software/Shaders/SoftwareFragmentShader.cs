using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareFragmentShader : SoftwareShader {
	uint ColorOutputId;
	public SoftwareFragmentShader ( SpirvCompiler compiler, ExecutionModel model ) : base( compiler, model ) {
		ColorOutputId = OutputIdByLocation[OutputsByLocation.First( x => x.Value.Base is RuntimeVector4Type<float> ).Key];
	}

	public FragmentShaderOutput Execute ( ShaderMemory memory ) {
		loadConstants( ref memory );
		Entry.Call( memory );

		var colorAddress = memory.Read<int>( GlobalScope.VariableInfo[ColorOutputId].Address );
		var color = memory.Read<ColorRgba<float>>( colorAddress );
		return new() {
			Color = color
		};
	}
}

public struct FragmentShaderOutput {
	public ColorRgba<float> Color;
}