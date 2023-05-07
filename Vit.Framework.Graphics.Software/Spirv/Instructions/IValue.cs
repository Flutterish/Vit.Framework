using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public interface IValue {
	DataType Type { get; }
}

public interface IAssignable {
	DataType Type { get; }
}