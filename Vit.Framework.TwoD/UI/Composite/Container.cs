using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;

namespace Vit.Framework.TwoD.UI.Composite;

public class Container : Container<UIComponent> { }
public class Container<T> : CompositeUIComponent<T, ContainerChildData<T>> where T : UIComponent {
	new public IReadOnlyList<T> Children {
		get => this;
		init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
	}
	public IEnumerable<T> ChildrenEnumerable {
		get => this;
		init {
			foreach ( var i in value )
				AddInternalChild( i );
		}
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

	public void NoUnloadRemoveChild ( T child ) {
		NoUnloadRemoveInternalChild( child );
	}
	public void NoUnloadRemoveChildAt ( int index ) {
		NoUnloadRemoveInternalChildAt( index );
	}

	public void ClearChildren () {
		ClearInternalChildren();
	}

	public void NoUnloadClearChildren () {
		NoUnloadClearInternalChildren();
	}

	public void DisposeChildren ( RenderThreadScheduler disposeScheduler ) {
		DisposeInternalChildren( disposeScheduler );
	}

	/// <inheritdoc cref="InternalContainer{T}.IsMaskingActive"/>
	new public bool IsMaskingActive {
		get => base.IsMaskingActive;
		set => base.IsMaskingActive = value;
	}
	/// <inheritdoc cref="InternalContainer{T}.CornerExponents"/>
	new public Corners<float> CornerExponents {
		get => base.CornerExponents;
		set => base.CornerExponents = value;
	}
	/// <inheritdoc cref="InternalContainer{T}.CornerRadii"/>
	new public Corners<Axes2<float>> CornerRadii {
		get => base.CornerRadii;
		set => base.CornerRadii = value;
	}
}
