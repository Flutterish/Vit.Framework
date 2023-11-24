using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;

namespace Vit.Framework.TwoD.UI;

public class Container<T> : CompositeUIComponent<T> where T : UIComponent {
	new public IReadOnlyList<T> Children {
		get => base.Children;
		init => base.Children = value;
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

	public void ClearChildren () {
		ClearInternalChildren();
	}

	public void DisposeChildren ( RenderThreadScheduler disposeScheduler ) {
		DisposeInternalChildren( disposeScheduler );
	}

	/// <inheritdoc cref="CompositeUIComponent{T}.IsMaskingActive"/>
	new public bool IsMaskingActive {
		get => base.IsMaskingActive;
		set => base.IsMaskingActive = value;
	}
	/// <inheritdoc cref="CompositeUIComponent{T}.CornerExponents"/>
	new public Corners<float> CornerExponents {
		get => base.CornerExponents;
		set => base.CornerExponents = value;
	}
	/// <inheritdoc cref="CompositeUIComponent{T}.CornerRadii"/>
	new public Corners<Axes2<float>> CornerRadii {
		get => base.CornerRadii;
		set => base.CornerRadii = value;
	}
}
