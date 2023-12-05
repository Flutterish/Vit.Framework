namespace Vit.Framework.TwoD.UI.Composite;

public abstract class InternalContainer : InternalContainer<UIComponent> { }
public abstract class InternalContainer<T> : CompositeUIComponent<T, ContainerChildData<T>> where T : UIComponent {
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
