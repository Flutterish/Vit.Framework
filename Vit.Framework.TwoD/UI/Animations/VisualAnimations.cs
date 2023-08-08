using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;

namespace Vit.Framework.TwoD.UI.Animations;

public class AlphaTintAnimation : Animation<IHasAlphaTint, ColorRgba<float>> {
	public AlphaTintAnimation ( IHasAlphaTint target, ColorRgba<float> endValue, double startTime, double endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

	protected override ColorRgba<float> GetValue () {
		return Target.Tint;
	}

	protected override void SetValue ( ColorRgba<float> value ) {
		Target.Tint = value;
	}

	protected override ColorRgba<float> Interpolate ( double t ) {
		return StartValue.Interpolate( EndValue, (float)t );
	}

	static readonly IReadOnlyList<AnimationDomain> domains = new[] { IHasTint.AnimationDomain, IHasAlpha.AnimationDomain };
	public override IReadOnlyList<AnimationDomain> Domains => domains;
}

public class TintAnimation : Animation<IHasTint, ColorRgb<float>> {
	public TintAnimation ( IHasTint target, ColorRgb<float> endValue, double startTime, double endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

	protected override ColorRgb<float> GetValue () {
		return Target.Tint;
	}

	protected override void SetValue ( ColorRgb<float> value ) {
		Target.Tint = value;
	}

	protected override ColorRgb<float> Interpolate ( double t ) {
		return StartValue.Interpolate( EndValue, (float)t );
	}

	static readonly IReadOnlyList<AnimationDomain> domains = new[] { IHasTint.AnimationDomain };
	public override IReadOnlyList<AnimationDomain> Domains => domains;
}

public class AlphaAnimation : Animation<IHasAlpha, float> {
	public AlphaAnimation ( IHasAlpha target, float endValue, double startTime, double endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

	protected override float GetValue () {
		return Target.Alpha;
	}

	protected override void SetValue ( float value ) {
		Target.Alpha = value;
	}

	protected override float Interpolate ( double t ) {
		var time = (float)t;
		return (1 - time) * StartValue + time * EndValue;
	}

	static readonly IReadOnlyList<AnimationDomain> domains = new[] { IHasAlpha.AnimationDomain };
	public override IReadOnlyList<AnimationDomain> Domains => domains;
}

public static class VisualAnimations {
	public static AnimationSequence<T> FadeTo<T> ( this AnimationSequence<T> sequence, float alpha, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.Add( new AlphaAnimation( sequence.Source, alpha, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FadeIn<T> ( this AnimationSequence<T> sequence, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.FadeTo( 1, duration, easing );
	public static AnimationSequence<T> FadeOut<T> ( this AnimationSequence<T> sequence, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.FadeTo( 0, duration, easing );

	public static AnimationSequence<T> FadeColour<T> ( this AnimationSequence<T> sequence, ColorRgb<float> tint, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasTint
		=> sequence.Add( new TintAnimation( sequence.Source, tint, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FlashColour<T> ( this AnimationSequence<T> sequence, ColorRgb<float> flashTint, ColorRgb<float> tint, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasTint
		=> sequence.FadeColour( flashTint, 0 ).Then().FadeColour( tint, duration, easing );

	public static AnimationSequence<T> FadeColour<T> ( this AnimationSequence<T> sequence, ColorRgba<float> tint, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlphaTint
		=> sequence.Add( new AlphaTintAnimation( sequence.Source, tint, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FlashColour<T> ( this AnimationSequence<T> sequence, ColorRgba<float> flashTint, ColorRgba<float> tint, double duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlphaTint
		=> sequence.FadeColour( flashTint, 0 ).Then().FadeColour( tint, duration, easing );
}
