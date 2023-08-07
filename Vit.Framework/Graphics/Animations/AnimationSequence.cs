﻿namespace Vit.Framework.Graphics.Animations;

public struct AnimationSequence<TSource> where TSource : IHasAnimationTimeline {
	public required double StartTime { get; init; }
	public required double EndTime { get; init; }
	public required TSource Source { get; init; }

	public AnimationSequence<TSource> Add ( Animation animation ) {
		Source.AnimationTimeline.Add( animation );
		return this with { EndTime = animation.EndTime };
	}

	public AnimationSequence<TOtherSource> AnimateOther<TOtherSource> ( TOtherSource source ) where TOtherSource : IHasAnimationTimeline {
		return new() { Source = source, StartTime = StartTime, EndTime = StartTime };
	}

	public AnimationSequence<TSource> Then () {
		return this with { StartTime = EndTime };
	}

	public AnimationSequence<TSource> Wait ( double time ) {
		return this with { StartTime = StartTime + time, EndTime = StartTime + time };
	}
}