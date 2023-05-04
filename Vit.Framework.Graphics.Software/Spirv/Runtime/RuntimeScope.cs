using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Textures;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeScope {
	public required ShaderOpaques Opaques;
	public readonly Dictionary<uint, VariableInfo> VariableInfo = new();
	public int CodePointer;
}

public class ShaderOpaques {
	public Dictionary<uint, Texture> Samplers = new();
}