using Vit.Framework.Collections;

namespace Vit.Framework.Graphics.Animations;

public class AnimationTimeline {
	/// <summary>
	/// If <see langword="true"/>, finished animations will not be removed.
	/// </summary>
	public bool Rewindable {
		get => animations.SeekBehaviour == SeekBehaviour.Rewind;
		set => animations.SeekBehaviour = value ? SeekBehaviour.Rewind : SeekBehaviour.Ignore;
	}

	public double CurrentTime { get => animations.CurrentTime; init => animations.CurrentTime = value; }
	Timeline<Animation> animations = new() { SeekBehaviour = SeekBehaviour.Ignore };
	public AnimationTimeline () {
		animations.EventStarted = onStarted;
		animations.EventStartRewound = onStartRewound;
		animations.EventEnded = onEnded;
		animations.EventEndRewound = onEndRewound;
	}

	public void Update ( double time ) {
		animations.SeekTo( time );
		foreach ( var (domain, list) in animationsByDomain ) {
			tryUpdate( list.Last?.Value, time );
		}
	}

	void tryUpdate ( Animation? animation, double time ) {
		if ( animation is null || time > animation.EndTime || time > animation.InterruptedAt )
			return;

		animation.Update( time );
	}

	// values are ordered from oldest (first node) to newest (last node) by start time.
	// newer animations "interrupt" older animations, even when they end before an older animation.
	Dictionary<AnimationDomain, LinkedList<Animation>> animationsByDomain = new();
	void onStarted ( Timeline<Animation>.Event @event ) {
		var animation = @event.Value;
		foreach ( var i in animation.Domains ) {
			if ( !animationsByDomain.TryGetValue( i, out var current ) )
				animationsByDomain.Add( i, current = new() );

			current.Last?.Value.OnInterrupted( animation.StartTime );
			tryUpdate( current.Last?.Value, animation.StartTime );
			current.AddLast( animation );
		}

		animation.OnStarted();
	}
	void onStartRewound ( Timeline<Animation>.Event @event ) {
		var animation = @event.Value;
		foreach ( var i in animation.Domains ) {
			var current = animationsByDomain[i];
			current.RemoveLast();
		}

		animation.OnStartRewound();
	}

	void onEnded ( Timeline<Animation>.Event @event ) {
		if ( !Rewindable )
			animations.Remove( @event );

		var animation = @event.Value;

		foreach ( var i in animation.Domains ) {
			var current = animationsByDomain[i];
			current.Remove( animation );
		}

		if ( animation.EndTime <= animation.InterruptedAt )
			animation.OnEnded();
	}
	void onEndRewound ( Timeline<Animation>.Event @event ) {
		var animation = @event.Value;

		foreach ( var i in animation.Domains ) {
			var current = animationsByDomain[i];

			var node = current.First;
			while ( node != null && node.Value.StartTime <= animation.StartTime ) {
				node = node.Next;
			}

			if ( node == null )
				current.AddLast( animation );
			else
				current.AddBefore( node, animation );
		}

		if ( animation.EndTime <= animation.InterruptedAt )
			animation.OnEndRewound();
	}

	public void Add ( Animation animation ) {
		if ( !Rewindable && animation.StartTime < CurrentTime )
			throw new InvalidOperationException( "Cannot insert an in-progress or finished animation to a non-rewindable timeline" );

		animations.Add( animation, animation.StartTime, animation.EndTime );
	}
}

public interface IHasAnimationTimeline {
	AnimationTimeline AnimationTimeline { get; }
}

public static class IHasAnimationTimelineExtensions {
	public static AnimationSequence<TSource> Animate<TSource> ( this TSource source ) where TSource : IHasAnimationTimeline {
		return new() { Source = source, StartTime = source.AnimationTimeline.CurrentTime, EndTime = source.AnimationTimeline.CurrentTime };
	}

	/// <summary>
	/// Instantly finishes all animations, if <see cref="AnimationTimeline.Rewindable"/> is <see langword="false"/>.
	/// </summary>
	public static void FinishAnimations ( this IHasAnimationTimeline source ) {
		var time = source.AnimationTimeline.CurrentTime;
		source.AnimationTimeline.Update( double.PositiveInfinity );
		source.AnimationTimeline.Update( time );
	}
}