using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Animations;

public struct AnimationSequence<TSource> where TSource : ICanBeAnimated {
	public required Millis StartTime { get; init; }
	public required Millis EndTime { get; init; }
	public required TSource Source { get; init; }

	/// <summary>
	/// Adds an animation to the sequence.
	/// </summary>
	public AnimationSequence<TSource> Add ( Animation animation ) {
		Source.AddAnimation( animation );
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
	public AnimationSequence<TSource> Delay ( Millis time ) {
		return this with { StartTime = StartTime + time, EndTime = StartTime + time };
	}

	/// <summary>
	/// Creates an animation sequence for a different animatable object, with the same timing properties as this one.
	/// </summary>
	public AnimationSequence<TOtherSource> AnimateOther<TOtherSource> ( TOtherSource source ) where TOtherSource : ICanBeAnimated {
		return new() { Source = source, StartTime = StartTime, EndTime = StartTime };
	}
}