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
	public double InterruptedAt { get; private set; } = double.PositiveInfinity;
	public void OnInterrupted ( double time ) {
		InterruptedAt = double.Min( InterruptedAt, time );
	}

	public abstract void Update ( double time );
	public abstract void OnStarted ();
	public abstract void OnStartRewound ();
	public abstract void OnEnded ();
	public abstract void OnEndRewound ();
}

public class AnimationDomain {
	public required string Name { get; init; }

	public override string ToString () {
		return Name;
	}
}

public abstract class Animation<TTarget, TValue> : Animation where TTarget : class {
	public readonly TTarget Target;
	public readonly EasingFunction Easing;
	protected TValue StartValue { get; private set; } = default!;
	protected TValue EndValue { get; private set; }
	bool hasStarted;

	public Animation ( TTarget target, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( startTime, endTime ) {
		Target = target;
		EndValue = endValue;
		Easing = easing;
	}

	protected abstract TValue GetValue ();
	protected abstract void SetValue ( TValue value );
	protected abstract TValue Interpolate ( double t );
	public sealed override void Update ( double time ) {
		SetValue( Interpolate( Duration == 0 ? 1 : Easing((time - StartTime) / Duration) ) );
	}

	public sealed override void OnStarted () {
		hasStarted = true;
		StartValue = GetValue();
	}
	public sealed override void OnStartRewound () {
		SetValue( StartValue! );
	}

	public sealed override void OnEnded () {
		SetValue( Interpolate( 1 ) );
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

public abstract class DynamicAnimation<TTarget, TValue> : Animation where TTarget : class {
	public readonly TTarget Target;
	public readonly EasingFunction Easing;
	protected TValue StartValue { get; private set; } = default!;
	protected TValue EndValue { get; private set; } = default!;
	bool hasStarted;

	public DynamicAnimation ( TTarget target, double startTime, double endTime, EasingFunction easing ) : base( startTime, endTime ) {
		Target = target;
		Easing = easing;
	}

	protected abstract TValue GetValue ();
	protected abstract TValue CreateEndValue ();
	protected abstract void SetValue ( TValue value );
	protected abstract TValue Interpolate ( double t );
	public sealed override void Update ( double time ) {
		SetValue( Interpolate( Duration == 0 ? 1 : Easing( (time - StartTime) / Duration ) ) );
	}

	public sealed override void OnStarted () {
		hasStarted = true;
		StartValue = GetValue();
		EndValue = CreateEndValue();
	}
	public sealed override void OnStartRewound () {
		SetValue( StartValue! );
	}

	public sealed override void OnEnded () {
		SetValue( Interpolate( 1 ) );
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