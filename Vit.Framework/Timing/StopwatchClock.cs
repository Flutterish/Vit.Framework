using System.Diagnostics;

namespace Vit.Framework.Timing;

public class StopwatchClock : IClock {
	Stopwatch stopwatch = new();
	public StopwatchClock () {
		stopwatch.Start();
	}
	
	public void Update () { // TODO prehaps the elapsed time should be limited in order not to have too big delta times
		var current = stopwatch.Elapsed.TotalMilliseconds;
		ElapsedTime = current - CurrentTime;
		CurrentTime = current;
	}

	public double ElapsedTime { get; private set; }
	public double CurrentTime { get; private set; }
}
