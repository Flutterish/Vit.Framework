namespace Vit.Framework.Timing;

public interface IClock {
	/// <summary>
	/// Absolute date and time at <c><see cref="ElapsedTime"/> = 0</c>.
	/// </summary>
	DateTime ClockEpoch { get; }

	/// <summary>
	/// Time elapsed during last "tick", in milliseconds.
	/// </summary>
	double ElapsedTime { get; }
	/// <summary>
	/// Total elasped in milliseconds.
	/// </summary>
	double CurrentTime { get; }

	public TimeSpan ElapsedTimeSpan => TimeSpan.FromMilliseconds( ElapsedTime );
	public TimeSpan CurrentTimeSpan => TimeSpan.FromMilliseconds( CurrentTime );

	public DateTime CurrentDateTime => ClockEpoch + CurrentTimeSpan;
}
