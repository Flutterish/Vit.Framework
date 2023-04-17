namespace Vit.Framework.Hierarchy;

public interface IComponent {
	IReadOnlyCompositeComponent<IComponent>? Parent { get; }
}

public interface IComponent<out TBase> : IComponent where TBase : IComponent<TBase> {
	new IReadOnlyCompositeComponent<TBase, TBase>? Parent { get; }

	IReadOnlyCompositeComponent<IComponent>? IComponent.Parent => (IReadOnlyCompositeComponent<IComponent>?)Parent;
}
