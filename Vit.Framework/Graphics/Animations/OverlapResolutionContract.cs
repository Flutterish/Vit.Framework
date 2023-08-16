using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Animations;

/// <summary>
/// A contract for animations which determines how to behave when multiple animations in the same domain try to play at once.
/// </summary>
/// <remarks>
/// The contract of the most recent animation will be used to resolve an overlap - if any animation down the line has a different contract,
/// it will simply be interrupted and not included in the resolution.
/// </remarks>
public abstract class OverlapResolutionContract {
	/// <summary>
	/// Updates the animations.
	/// </summary>
	/// <param name="animations">Overlapping animations. Must be active at <c><paramref name="time"/></c>. Must be ordered descending by start time, and the first element is the active animation.</param>
	/// <param name="time">Time parameter.</param>
	public abstract void Update ( IEnumerable<Animation> animations, Millis time );
}
