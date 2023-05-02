namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public class RuntimeScope { // TODO instead of a scope like this, we should just use a span of memory
	public readonly Dictionary<uint, IVariable> Variables = new();
	public int CodePointer;
}
