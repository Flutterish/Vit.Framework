using Vit.Framework.Mathematics;

namespace Vit.Framework.Timing;

public interface IClock {
	/// <summary>
	/// Absolute date and time at <c><see cref="ElapsedTime"/> = 0</c>.
	/// </summary>
	DateTime ClockEpoch { get; }

	/// <summary>
	/// Time elapsed during last "tick", in milliseconds.
	/// </summary>
	Millis ElapsedTime { get; }
	/// <summary>
	/// Total elasped in milliseconds.
	/// </summary>
	Millis CurrentTime { get; }

	public DateTime CurrentDateTime => ClockEpoch + CurrentTime;
}
