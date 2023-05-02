using Vit.Framework.Graphics.Software.Spirv;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Shaders;

public class SoftwareShader {
	public readonly SpirvCompiler Compiler;
	public readonly ExecutionModel ExecutionModel;

	public readonly Dictionary<uint, IRuntimeType> InputTypesByLocation = new();
	public SoftwareShader ( SpirvCompiler compiler, ExecutionModel model ) {
		Compiler = compiler;
		ExecutionModel = model;

		var entry = compiler.EntryPointsByModel[model];
		var interfaces = entry.InterfaceIds.Select( x => compiler.Variables[x] );
		var inputInterfaces = interfaces.Where( x => x.StorageClass == StorageClass.Input ).Select( x => ((PointerType)x.Type).Type ).ToArray();
		var outputInterfaces = interfaces.Where( x => x.StorageClass == StorageClass.Output ).Select( x => ((PointerType)x.Type).Type ).ToArray();

		foreach ( var i in inputInterfaces ) {
			//InputTypesByLocation.Add( compiler.Decorations[i] );
		}
	}
}
