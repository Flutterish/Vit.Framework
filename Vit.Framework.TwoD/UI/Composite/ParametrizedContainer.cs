using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;

namespace Vit.Framework.TwoD.UI.Composite;

public interface IParametrizedContainer<in T, TParam> where TParam : unmanaged {
	TParam GetLayoutParameters ( T child );
	void UpdateLayoutParameters ( T child, TParam param );
	void UpdateLayoutParameters ( T child, Func<TParam, TParam> transformer );
	void UpdateLayoutParameters<TData> ( T child, TData data, Func<TParam, TData, TParam> transformer );
}

public abstract class ParametrizedContainer<T, TParam> : ParametrizedContainer<T, TParam, DefaultChildPolicy<T>> where T : UIComponent where TParam : unmanaged { }
public abstract class ParametrizedContainer<T, TParam, TChildPolicy> : CompositeUIComponent<T, ParametrizedChildData<T, TParam>, TChildPolicy>, IParametrizedContainer<T, TParam> 
	where T : UIComponent 
	where TParam : unmanaged 
	where TChildPolicy : struct, IChildPolicy<T>
{
	public IReadOnlyList<ParametrizedChildData<T, TParam>> LayoutChildren {
		get => InternalChildren;
		init => InternalChildren = value;
	}
	public IEnumerable<ParametrizedChildData<T, TParam>> LayoutChildrenEnumerable {
		get => InternalChildren;
		init {
			foreach ( var i in value )
				AddChild( i.Child, i.Parameter );
		}
	}

	public void UpdateLayoutParameters ( T child, TParam param ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		ref var data = ref ChildDataAt( child.Depth );
		var old = data.Parameter;
		data.Parameter = param;
		OnChildParameterUpdated( child, old, param );
	}
	public void UpdateLayoutParameters ( T child, Func<TParam, TParam> transformer ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		ref var data = ref ChildDataAt( child.Depth );
		var old = data.Parameter;
		var param = data.Parameter = transformer( old );
		OnChildParameterUpdated( child, old, param );
	}
	public void UpdateLayoutParameters<TData> ( T child, TData data, Func<TParam, TData, TParam> transformer ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		ref var _data = ref ChildDataAt( child.Depth );
		var old = _data.Parameter;
		var param = _data.Parameter = transformer( old, data );
		OnChildParameterUpdated( child, old, param );
	}

	/// <summary>
	/// Performs the necessary invalidations after a parameter change.
	/// </summary>
	protected abstract void OnChildParameterUpdated ( T child, TParam? previous, TParam? current );

	public TParam GetLayoutParameters ( T child ) {
		return InternalChildren[child.Depth].Parameter;
	}

	public void AddChild ( T child, TParam param ) {
		AddInternalChild( (child, param) );
		OnChildParameterUpdated( child, null, param );
	}
	public void InsertChild ( int index, T child, TParam param ) {
		InsertInternalChild( index, (child, param) );
		OnChildParameterUpdated( child, null, param );
	}

	public void RemoveChild ( T child ) {
		var param = InternalChildren[child.Depth].Parameter;
		RemoveInternalChild( child );
		OnChildParameterUpdated( child, param, null );
	}
	public void RemoveChildAt ( int index ) {
		var (child, param) = InternalChildren[index];
		RemoveInternalChildAt( index );
		OnChildParameterUpdated( child, param, null );
	}

	public void NoUnloadRemoveChild ( T child ) {
		var param = InternalChildren[child.Depth].Parameter;
		NoUnloadRemoveInternalChild( child );
		OnChildParameterUpdated( child, param, null );
	}
	public void NoUnloadRemoveChildAt ( int index ) {
		var (child, param) = InternalChildren[index];
		NoUnloadRemoveInternalChildAt( index );
		OnChildParameterUpdated( child, param, null );
	}

	public void ClearChildren () {
		foreach ( var (i, param) in InternalChildren ) {
			OnChildParameterUpdated( i, param, null );
		}
		ClearInternalChildren();
	}

	public void NoUnloadClearChildren () {
		foreach ( var (i, param) in InternalChildren ) {
			OnChildParameterUpdated( i, param, null );
		}
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

public static class ParametrizedContainerExtensions {
	public class ParametersAnimation<T, TContainer, TParam> : Animation<T, TParam>
		where TContainer : IParametrizedContainer<T, TParam>
		where T : UIComponent
		where TParam : unmanaged, IInterpolatable<TParam, float> // TODO an overload for doubles
	{
		TContainer container;
		public ParametersAnimation ( T target, TContainer container, TParam endValue, Millis startTime, Millis endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) {
			this.container = container;
		}

		protected override TParam GetValue () {
			return container.GetLayoutParameters( Target );
		}

		public override void SetValue ( TParam value ) {
			container.UpdateLayoutParameters( Target, value );
		}

		public override TParam Interpolate ( TParam from, TParam to, double t ) {
			return from.Lerp( to, (float)t );
		}

		public override IReadOnlyList<AnimationDomain> Domains => LayoutParametersAnimationDomains;
	}

	public class ParametersTransformerAnimation<T, TContainer, TParam> : DynamicAnimation<T, TParam>
		where TContainer : IParametrizedContainer<T, TParam>
		where T : UIComponent
		where TParam : unmanaged, IInterpolatable<TParam, float> // TODO an overload for doubles
	{
		TContainer container;
		Func<TParam, TParam> transformer;
		public ParametersTransformerAnimation ( T target, TContainer container, Func<TParam, TParam> transformer, Millis startTime, Millis endTime, EasingFunction easing ) : base( target, startTime, endTime, easing ) {
			this.container = container;
			this.transformer = transformer;
		}

		protected override TParam GetValue () {
			return container.GetLayoutParameters( Target );
		}
		protected override TParam CreateEndValue () {
			return transformer( StartValue );
		}

		public override void SetValue ( TParam value ) {
			container.UpdateLayoutParameters( Target, value );
		}

		public override TParam Interpolate ( TParam from, TParam to, double t ) {
			return from.Lerp( to, (float)t );
		}

		public override IReadOnlyList<AnimationDomain> Domains => LayoutParametersAnimationDomains;
	}

	public static readonly AnimationDomain LayoutParametersAnimationDomain = new() { Name = "Layout Parameters" };
	public static readonly IReadOnlyList<AnimationDomain> LayoutParametersAnimationDomains = new[] { LayoutParametersAnimationDomain };

	public static AnimationSequence<TContainer> ChangeLayoutParameters<TContainer, T, TParam> ( this AnimationSequence<TContainer> sequence, T child, TParam goal, Millis duration, EasingFunction? easing = null )
		where TContainer : IParametrizedContainer<T, TParam>, ICanBeAnimated
		where T : UIComponent
		where TParam : unmanaged, IInterpolatable<TParam, float> {
		return sequence.AnimateOther( child )
			.Add( new ParametersAnimation<T, TContainer, TParam>( child, sequence.Source, goal, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) )
			.AnimateOther( sequence.Source );
	}

	public static AnimationSequence<TContainer> ChangeLayoutParameters<TContainer, T, TParam> ( this AnimationSequence<TContainer> sequence, T child, Func<TParam, TParam> transformer, Millis duration, EasingFunction? easing = null )
		where TContainer : IParametrizedContainer<T, TParam>, ICanBeAnimated
		where T : UIComponent
		where TParam : unmanaged, IInterpolatable<TParam, float> {
		return sequence.AnimateOther( child )
			.Add( new ParametersTransformerAnimation<T, TContainer, TParam>( child, sequence.Source, transformer, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) )
			.AnimateOther( sequence.Source );
	}
}