using Vit.Framework.Mathematics;

namespace Vit.Framework.Input.Events;

/// <summary>
/// Positional events' propagation is culled by 2D hitboxes.
/// </summary>
public interface IPositionalEvent {
	Point2<float> EventPosition { get; }
}
