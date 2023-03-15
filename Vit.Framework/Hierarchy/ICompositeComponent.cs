namespace Vit.Framework.Hierarchy;

public interface IReadOnlyCompositeComponent<out T> : IComponent<IReadOnlyCompositeComponent<T>> where T : IComponent<T> {
	IEnumerable<T> Children { get; }
}

public interface ICompositeComponent<T> : IComponent<ICompositeComponent<T>>, IReadOnlyCompositeComponent<T> where T : IComponent<T> {
	void AddChild ( T child );
	void AddChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			AddChild( i );
	}

	bool RemoveChild ( T child );
	void RemoveChildren ( IEnumerable<T> children ) {
		foreach ( var i in children )
			RemoveChild( i );
	}

	void ClearChildren ();
}
