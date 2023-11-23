using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Mathematics;

namespace Vit.Framework.TwoD.UI.Animations;

public class TintAnimation : Animation<IHasTint, ColorRgb<float>> {
	public TintAnimation ( IHasTint target, ColorRgb<float> endValue, Millis startTime, Millis endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

	protected override ColorRgb<float> GetValue () {
		return Target.Tint;
	}

	public override void SetValue ( ColorRgb<float> value ) {
		Target.Tint = value;
	}

	public override ColorRgb<float> Interpolate ( ColorRgb<float> from, ColorRgb<float> to, double t ) {
		return from.Interpolate( to, (float)t );
	}

	static readonly IReadOnlyList<AnimationDomain> domains = new[] { IHasTint.AnimationDomain };
	public override IReadOnlyList<AnimationDomain> Domains => domains;
}

public class AlphaAnimation : Animation<IHasAlpha, float> {
	public AlphaAnimation ( IHasAlpha target, float endValue, Millis startTime, Millis endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

	protected override float GetValue () {
		return Target.Alpha;
	}

	public override void SetValue ( float value ) {
		Target.Alpha = value;
	}

	public override float Interpolate ( float from, float to, double t ) {
		var time = (float)t;
		return (1 - time) * from + time * to;
	}

	static readonly IReadOnlyList<AnimationDomain> domains = new[] { IHasAlpha.AnimationDomain };
	public override IReadOnlyList<AnimationDomain> Domains => domains;
}

public static class VisualAnimations {
	public static AnimationSequence<T> FadeTo<T> ( this AnimationSequence<T> sequence, float alpha, Millis duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.Add( new AlphaAnimation( sequence.Source, alpha, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FadeIn<T> ( this AnimationSequence<T> sequence, Millis duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.FadeTo( 1, duration, easing );
	public static AnimationSequence<T> FadeOut<T> ( this AnimationSequence<T> sequence, Millis duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasAlpha
		=> sequence.FadeTo( 0, duration, easing );

	public static AnimationSequence<T> FadeColour<T> ( this AnimationSequence<T> sequence, ColorRgb<float> tint, Millis duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasTint
		=> sequence.Add( new TintAnimation( sequence.Source, tint, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FlashColour<T> ( this AnimationSequence<T> sequence, ColorRgb<float> flashTint, ColorRgb<float> tint, Millis duration, EasingFunction? easing = null ) where T : ICanBeAnimated, IHasTint
		=> sequence.FadeColour( flashTint, 0.Millis() ).Then().FadeColour( tint, duration, easing );
}
