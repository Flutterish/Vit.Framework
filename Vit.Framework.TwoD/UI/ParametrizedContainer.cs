using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.Rendering.Masking;

namespace Vit.Framework.TwoD.UI;

public interface IParametrizedContainer<in T, TParam> where TParam : struct {
	TParam GetLayoutParameters ( T child );
	void UpdateLayoutParameters ( T child, TParam param );
	void UpdateLayoutParameters ( T child, Func<TParam, TParam> transformer );
}

public abstract class ParametrizedContainer<T, TParam> : CompositeUIComponent<T>, IParametrizedContainer<T, TParam> where T : UIComponent where TParam : struct {
	List<TParam> parameters = new();

	public virtual IEnumerable<(T child, TParam @param)> LayoutChildren {
		get {
			foreach ( var i in Children ) {
				yield return (i, parameters[i.Depth]);
			}
		}
		init {
			foreach ( var (child, param) in value ) {
				AddChild( child, param );
			}
		}
	}

	public void UpdateLayoutParameters ( T child, TParam param ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		var old = parameters[child.Depth];
		parameters[child.Depth] = param;
		OnChildParameterUpdated( child, old, param );
	}
	public void UpdateLayoutParameters ( T child, Func<TParam, TParam> transformer ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		var old = parameters[child.Depth];
		var param = parameters[child.Depth] = transformer( old );
		OnChildParameterUpdated( child, old, param );
	}
	public void UpdateLayoutParameters<TData> ( T child, TData data, Func<TParam, TData, TParam> transformer ) {
		if ( child.Parent != this )
			throw new InvalidOperationException( "Child does not belong to this parent" );

		var old = parameters[child.Depth];
		var param = parameters[child.Depth] = transformer( old, data );
		OnChildParameterUpdated( child, old, param );
	}

	/// <summary>
	/// Performs the necessary invalidations after a parameter change.
	/// </summary>
	protected abstract void OnChildParameterUpdated ( T child, TParam? previous, TParam? current );

	public TParam GetLayoutParameters ( T child ) {
		return parameters[child.Depth];
	}

	public void AddChild ( T child, TParam param ) {
		AddInternalChild( child );
		parameters.Add( param );
		OnChildParameterUpdated( child, null, param );
	}
	public void InsertChild ( int index, T child, TParam param ) {
		InsertInternalChild( index, child );
		parameters.Insert( index, param );
		OnChildParameterUpdated( child, null, param );
	}

	public void RemoveChild ( T child ) {
		var param = parameters[child.Depth];
		parameters.RemoveAt( child.Depth );
		RemoveInternalChild( child );
		OnChildParameterUpdated( child, param, null );
	}
	public void RemoveChildAt ( int index ) {
		var param = parameters[index];
		parameters.RemoveAt( index );
		var child = Children[index];
		RemoveInternalChildAt( index );
		OnChildParameterUpdated( child, param, null );
	}

	public void ClearChildren () {
		parameters.Clear();
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

public static class ParametrizedContainerExtensions {
	public class ParametersAnimation<T, TContainer, TParam> : Animation<T, TParam>
		where TContainer : IParametrizedContainer<T, TParam>
		where T : UIComponent
		where TParam : struct, IInterpolatable<TParam, float> // TODO an overload for doubles
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
		where TParam : struct, IInterpolatable<TParam, float> // TODO an overload for doubles
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
		where TParam : struct, IInterpolatable<TParam, float>
	{
		return sequence.AnimateOther( child )
			.Add( new ParametersAnimation<T, TContainer, TParam>( child, sequence.Source, goal, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) )
			.AnimateOther( sequence.Source );
	}

	public static AnimationSequence<TContainer> ChangeLayoutParameters<TContainer, T, TParam> ( this AnimationSequence<TContainer> sequence, T child, Func<TParam, TParam> transformer, Millis duration, EasingFunction? easing = null )
		where TContainer : IParametrizedContainer<T, TParam>, ICanBeAnimated
		where T : UIComponent
		where TParam : struct, IInterpolatable<TParam, float> {
		return sequence.AnimateOther( child )
			.Add( new ParametersTransformerAnimation<T, TContainer, TParam>( child, sequence.Source, transformer, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) )
			.AnimateOther( sequence.Source );
	}
}