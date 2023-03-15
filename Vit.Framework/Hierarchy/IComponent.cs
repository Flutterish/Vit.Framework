namespace Vit.Framework.Hierarchy;

public interface IComponent<out TSelf> where TSelf : IComponent<TSelf> {
	IReadOnlyCompositeComponent<TSelf>? Parent { get; }
}
