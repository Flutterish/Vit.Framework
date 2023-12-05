namespace Vit.Framework.Hierarchy;

public interface IComponent { // TODO do we really need this?
	IReadOnlyCompositeComponent<IComponent>? Parent { get; }
}

public interface IComponent<out TBase> : IComponent where TBase : IComponent<TBase> {
	new IReadOnlyCompositeComponent<TBase, TBase>? Parent { get; }

	IReadOnlyCompositeComponent<IComponent>? IComponent.Parent => (IReadOnlyCompositeComponent<IComponent>?)Parent;
}