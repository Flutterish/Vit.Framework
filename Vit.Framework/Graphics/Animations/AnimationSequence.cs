﻿namespace Vit.Framework.Graphics.Animations;

public struct AnimationSequence<TSource> where TSource : IHasAnimationTimeline {
	public required double StartTime { get; init; }
	public required double EndTime { get; init; }
	public required TSource Source { get; init; }

	/// <summary>
	/// Adds an animation to the sequence.
	/// </summary>
	public AnimationSequence<TSource> Add ( Animation animation ) {
		Source.AnimationTimeline.Add( animation );
		return this with { EndTime = animation.EndTime };
	}

	/// <summary>
	/// Sets the start time of future animations to the end time of the most recently added animation.
	/// </summary>
	public AnimationSequence<TSource> Then () {
		return this with { StartTime = EndTime };
	}

	/// <summary>
	/// Increments the start time of future animations by the given delay.
	/// </summary>
	public AnimationSequence<TSource> Delay ( double time ) {
		return this with { StartTime = StartTime + time, EndTime = StartTime + time };
	}

	/// <summary>
	/// Creates an animation sequence for a different animatable object, with the same timing properties as this one.
	/// </summary>
	public AnimationSequence<TOtherSource> AnimateOther<TOtherSource> ( TOtherSource source ) where TOtherSource : IHasAnimationTimeline {
		return new() { Source = source, StartTime = StartTime, EndTime = StartTime };
	}
}