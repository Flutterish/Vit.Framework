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
}

public abstract class Animation<TTarget, TValue> : Animation where TTarget : class {
	public readonly TTarget Target;
	public readonly TValue EndValue;
	protected TValue StartValue { get; private set; } = default!;

	protected Animation ( TTarget target, TValue endValue, double startTime, double endTime ) : base( startTime, endTime ) {
		Target = target;
		EndValue = endValue;
	}

	protected abstract TValue GetValue ();
	protected abstract void SetValue ( TValue value );
	protected abstract TValue Interpolate ( double t );
	public sealed override void Update ( double time ) {
		SetValue( Interpolate( Duration == 0 ? 1 : ((time - StartTime) / Duration) ) );
	}

	public sealed override void OnStarted () {
		StartValue = GetValue();
	}
	public sealed override void OnStartRewound () {
		SetValue( StartValue );
	}

	public sealed override void OnEnded () {
		SetValue( EndValue );
	}
	public sealed override void OnEndRewound () {
		
	}
}