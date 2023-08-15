using System.Diagnostics;

namespace Vit.Framework.Graphics.Animations;

public abstract class Animation {
	public readonly double StartTime;
	public readonly double EndTime;
	protected Animation ( double startTime, double endTime ) {
		StartTime = startTime;
		EndTime = endTime;
	}

	public double Duration => EndTime - StartTime;
	/// <summary>
	/// Domains of this animation - when 2 animations in the same domain play, the older one is interrupted.
	/// </summary>
	/// <remarks>
	/// There  must be at least one animation domain. If there are none, this animation will be ignored.
	/// </remarks>
	public abstract IReadOnlyList<AnimationDomain> Domains { get; }
	public virtual OverlapResolutionContract? OverlapResolutionContract => null;
	public double InterruptedAt { get; private set; } = double.PositiveInfinity;
	public Animation? InterruptedBy { get; private set; }
	public bool WasInterrupted => InterruptedBy != null;
	public void OnInterrupted ( double time, Animation by ) { // TODO not sure what happens if it gets interrupted in 2 different domains
		Debug.Assert( !WasInterrupted || (time == InterruptedAt && by == InterruptedBy), "I dont know how to handle this yet. Please dont get interrupted in 2 places at once." );

		InterruptedAt = time;
		InterruptedBy = by;
	}

	public abstract void Update ( double time );
	public abstract void OnStarted ();
	public abstract void OnStartRewound ();
	public abstract void OnEnded ();
	public abstract void OnEndRewound ();
}

public abstract class Animation<TTarget, TValue> : Animation, IValueInterpolatingAnimation<TValue> where TTarget : class {
	public readonly TTarget Target;
	public readonly EasingFunction Easing;
	public TValue StartValue { get; private set; } = default!;
	TValue interpolatedStartValue = default!;
	public TValue EndValue { get; private set; }
	bool hasStarted;

	public Animation ( TTarget target, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( startTime, endTime ) {
		Target = target;
		EndValue = endValue;
		Easing = easing;
	}

	protected abstract TValue GetValue ();
	public abstract void SetValue ( TValue value );
	public abstract TValue Interpolate ( TValue from, TValue to, double t );
	public override OverlapResolutionContract? OverlapResolutionContract => ValueInterpolatingAnimationOverlapResolutionContract<TValue>.Value;
	public sealed override void Update ( double time ) {
		SetValue( GetValue( time ) );
	}

	public TValue GetValue ( double time ) {
		return Interpolate( interpolatedStartValue, EndValue, Duration == 0 ? 1 : Easing( (time - StartTime) / Duration ) );
	}

	public sealed override void OnStarted () {
		hasStarted = true;
		interpolatedStartValue = StartValue = GetValue();
	}
	public void ChangeInterpolatedStartValue ( TValue value ) {
		interpolatedStartValue = value;
	}
	public sealed override void OnStartRewound () {
		SetValue( StartValue! );
	}

	public sealed override void OnEnded () {
		SetValue( Interpolate( interpolatedStartValue, EndValue, 1 ) );
	}
	public sealed override void OnEndRewound () {
		
	}

	public override string ToString () {
		if ( !hasStarted )
			return $"{GetType().Name} ({string.Join( ", ", Domains )}) Not Started -> {StringifyValue(EndValue)} in {Duration:N1}ms";
		else
			return $"{GetType().Name} ({string.Join( ", ", Domains)}) {StringifyValue(StartValue)} -> {StringifyValue(EndValue)} in {Duration:N1}ms";
	}

	protected virtual string StringifyValue ( TValue value ) => $"{value}";
}

public abstract class DynamicAnimation<TTarget, TValue> : Animation, IValueInterpolatingAnimation<TValue> where TTarget : class {
	public readonly TTarget Target;
	public readonly EasingFunction Easing;
	public TValue StartValue { get; private set; } = default!;
	TValue interpolatedStartValue = default!;
	public TValue EndValue { get; private set; } = default!;
	bool hasStarted;

	public DynamicAnimation ( TTarget target, double startTime, double endTime, EasingFunction easing ) : base( startTime, endTime ) {
		Target = target;
		Easing = easing;
	}

	protected abstract TValue GetValue ();
	protected abstract TValue CreateEndValue ();
	public abstract void SetValue ( TValue value );
	public abstract TValue Interpolate ( TValue from, TValue to, double t );
	public override OverlapResolutionContract? OverlapResolutionContract => ValueInterpolatingAnimationOverlapResolutionContract<TValue>.Value;
	public sealed override void Update ( double time ) {
		SetValue( GetValue( time ) );
	}

	public TValue GetValue ( double time ) {
		return Interpolate( interpolatedStartValue, EndValue, Duration == 0 ? 1 : Easing( (time - StartTime) / Duration ) );
	}
	public sealed override void OnStarted () {
		hasStarted = true;
		interpolatedStartValue = StartValue = GetValue();
		EndValue = CreateEndValue();
	}
	public void ChangeInterpolatedStartValue ( TValue value ) {
		interpolatedStartValue = value;
	}
	public sealed override void OnStartRewound () {
		SetValue( StartValue! );
	}

	public sealed override void OnEnded () {
		SetValue( Interpolate( interpolatedStartValue, EndValue, 1 ) );
	}
	public sealed override void OnEndRewound () {

	}

	public override string ToString () {
		if ( !hasStarted )
			return $"{GetType().Name} ({string.Join( ", ", Domains )}) Not Started -> {{Dynamic End Value}} in {Duration:N1}ms";
		else
			return $"{GetType().Name} ({string.Join( ", ", Domains )}) {StringifyValue( StartValue )} -> {StringifyValue( EndValue )} in {Duration:N1}ms";
	}

	protected virtual string StringifyValue ( TValue value ) => $"{value}";
}
