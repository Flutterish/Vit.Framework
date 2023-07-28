namespace Vit.Framework.Graphics.TwoD.UI;

public class Container<T> : CompositeUIComponent<T> where T : UIComponent {
	new public IReadOnlyList<T> Children {
		get => base.Children;
		set => base.Children = value;
	}

	public void AddChild ( T child ) {
		AddInternalChild( child );
	}
	public void InsertChild ( int index, T child ) {
		InsertInternalChild( index, child );
	}

	public void RemoveChild ( T child ) {
		RemoveInternalChild( child );
	}
	public void RemoveChildAt ( int index ) {
		RemoveInternalChildAt( index );
	}

	public void ClearChildren ( bool dispose ) {
		ClearInternalChildren( dispose );
	}
}
