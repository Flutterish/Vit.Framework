namespace Vit.Framework.Graphics.Software.Spirv.Runtime;

public interface IRuntimeType { }
public interface IRuntimeType<T> where T : unmanaged { }

public abstract class RuntimeType<T> : IRuntimeType<T> where T : unmanaged {
	
}
