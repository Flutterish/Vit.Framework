using System.Diagnostics;

namespace Vit.Framework.Timing;

public class StopwatchClock : IClock {
	Stopwatch stopwatch = new();
	public StopwatchClock () {
		stopwatch.Start();
		ClockEpoch = DateTime.Now;
	}
	
	public void Update () { // TODO prehaps the elapsed time should be limited in order not to have too big delta times
		var current = stopwatch.Elapsed.TotalMilliseconds;
		ElapsedTime = current - CurrentTime;
		CurrentTime = current;
	}

	public DateTime ClockEpoch { get; }
	public double ElapsedTime { get; private set; }
	public double CurrentTime { get; private set; }
}
