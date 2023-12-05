namespace Vit.Framework.TwoD.UI.Composite;

public abstract class InternalContainer : InternalContainer<UIComponent> { }
public abstract class InternalContainer<T> : InternalContainer<T, DefaultChildPolicy<T>> where T : UIComponent { }
public abstract class InternalContainer<T, TChildPolicy> : CompositeUIComponent<T, ContainerChildData<T>, TChildPolicy> 
	where T : UIComponent 
	where TChildPolicy : struct, IChildPolicy<T> 
{
	new public IReadOnlyList<T> Children {
		get => this;
		protected init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}
	public IEnumerable<T> ChildrenEnumerable {
		get => this;
		protected init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}
}
