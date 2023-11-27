using System.Diagnostics;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Timing;

public class StopwatchClock : IClock {
	Stopwatch stopwatch = new();
	public StopwatchClock () {
		stopwatch.Start();
		ClockEpoch = DateTime.Now;
	}
	
	public void Update () { // TODO prehaps the elapsed time should be limited in order not to have too big delta times
		var current = stopwatch.Elapsed.TotalMilliseconds.Millis();
		ElapsedTime = current - CurrentTime;
		CurrentTime = current;
	}

	public DateTime ClockEpoch { get; }
	public Millis ElapsedTime { get; private set; }
	public Millis CurrentTime { get; private set; }

	public override string ToString () {
		return $"{CurrentTime}; Elapsed = {ElapsedTime}";
	}
}
