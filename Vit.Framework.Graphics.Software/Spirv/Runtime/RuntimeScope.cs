using Vit.Framework.Graphics.Software.Shaders;

namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeScope {
	public readonly Dictionary<uint, VariableInfo> VariableInfo = new();
	public int CodePointer;
}
