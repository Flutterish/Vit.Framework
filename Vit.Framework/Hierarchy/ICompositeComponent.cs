using System.Collections;

namespace Vit.Framework.Hierarchy;

public static class HierarchyObserver {
	public delegate void ChildObserver<in TParent, in T> ( TParent parent, T child );

	public static IHierarchyObserverContract ObserveSubtree<T> ( 
		this IReadOnlyCompositeComponent<T> head,
		ChildObserver<IReadOnlyCompositeComponent<T>, T>? added,
		ChildObserver<IReadOnlyCompositeComponent<T>, T>? removed
	)
		where T : IComponent 
	{
		void onChildAdded ( IReadOnlyCompositeComponent<T> parent, T child ) {
			added?.Invoke( parent, child );
			if ( child is not IReadOnlyCompositeComponent<T> subtree )
				return;

			subtree.ChildAdded += onChildAdded;
			subtree.ChildRemoved += onChildRemoved;
			foreach ( var i in subtree.Children ) {
				onChildAdded( subtree, i );
			}
		}

		void onChildRemoved ( IReadOnlyCompositeComponent<T> parent, T child ) {
			if ( child is IReadOnlyCompositeComponent<T> subtree ) {
				foreach ( var i in subtree.Children.Reverse() ) {
					onChildRemoved( subtree, i );
				}
				subtree.ChildRemoved -= onChildRemoved;
				subtree.ChildRemoved -= onChildAdded;
			}

			removed?.Invoke( parent, child );
		}

		head.ChildAdded += onChildAdded;
		head.ChildRemoved += onChildRemoved;
		var contract = new Contract<T>() {
			Head = head,
			Added = onChildAdded,
			Removed = onChildRemoved,
			IsActive = true
		};

		foreach ( var i in head.Children ) {
			onChildAdded( head, i );
		}

		return contract;
	}

	struct Contract<T> : IHierarchyObserverContract where T : IComponent {
		public required IReadOnlyCompositeComponent<T> Head;
		public required ChildObserver<IReadOnlyCompositeComponent<T>, T> Added;
		public required ChildObserver<IReadOnlyCompositeComponent<T>, T> Removed;
		public required bool IsActive;

		public void Unsubscribe () {
			if ( !IsActive )
				throw new InvalidOperationException( "Contract has already been unsubscribed from" );

			IsActive = false;

			foreach ( var i in Head.Children ) {
				Removed( Head, i );
			}
		}
	}
}

public interface IHierarchyObserverContract {
	void Unsubscribe ();
}

public interface IReadOnlyCompositeComponent<out T> : IComponent where T : IComponent {
	IReadOnlyList<T> Children { get; }

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? ChildAdded;
	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? ChildRemoved;
}

public static class IReadOnlyCompositeComponentExtensions {
	public static IEnumerable<T> EnumerateSubtree<T> ( this IReadOnlyCompositeComponent<T> self ) where T : IComponent {
		foreach ( var child in self.Children ) {
			yield return child;
			if ( child is not IReadOnlyCompositeComponent<T> subtree )
				continue;

			foreach ( var subchild in subtree.EnumerateSubtree() ) {
				yield return subchild;
			}
		}
	}
}

public interface IReadOnlyCompositeComponent<out TBase, out T> : IComponent<TBase>, IReadOnlyCompositeComponent<T>
	where TBase : IComponent<TBase> 
	where T : IComponent<TBase> 
{
	new event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<TBase, T>, T>? ChildAdded;
	new event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<TBase, T>, T>? ChildRemoved;

	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? IReadOnlyCompositeComponent<T>.ChildAdded {
		add => ChildAdded += value;
		remove => ChildAdded -= value;
	}
	event HierarchyObserver.ChildObserver<IReadOnlyCompositeComponent<T>, T>? IReadOnlyCompositeComponent<T>.ChildRemoved {
		add => ChildRemoved += value;
		remove => ChildRemoved -= value;
	}
}

public interface ICompositeComponent<in T> : IComponent where T : IComponent {
	void AddChild ( T child );
	void InsertChild ( T child, int index );
	bool RemoveChild ( T child );
	void RemoveChildAt ( int index );
	void ClearChildren ();
}

public static class ICompositeComponentExtensions {
	public static void RemoveChildren<T> ( this ICompositeComponent<T> self, params T[] children ) where T : IComponent
		=> self.RemoveChildren( children.AsEnumerable() );
	public static void RemoveChildren<T> ( this ICompositeComponent<T> self, IEnumerable<T> children ) where T : IComponent {
		foreach ( var i in children )
			self.RemoveChild( i );
	}

	public static void AddChildren<T> ( this ICompositeComponent<T> self, params T[] children ) where T : IComponent
		=> self.AddChildren( children.AsEnumerable() );
	public static void AddChildren<T> ( this ICompositeComponent<T> self, IEnumerable<T> children ) where T : IComponent {
		foreach ( var i in children )
			self.AddChild( i );
	}
}

public interface ICompositeComponent<in T, out TChild> : ICompositeComponent<T>, IReadOnlyCompositeComponent<TChild> 
	where T : TChild, IComponent
	where TChild : IComponent
{

}

public interface ICompositeComponent<out TBase, in T, out TChild> : IComponent<TBase>, ICompositeComponent<T, TChild>
	where TBase : IComponent<TBase> 
	where T : TChild, IComponent<TBase>
	where TChild : IComponent<TBase> 
{

}