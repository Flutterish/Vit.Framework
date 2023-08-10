using System.Diagnostics;

namespace Vit.Framework.Performance;

public class UpdateCounter {
	public TimeSpan MeasuredPeriod = TimeSpan.FromSeconds( 1 );

	TimeSpan totalTime;
	Queue<TimeSpan> updateFrames = new();
	Stopwatch stopwatch = new();

	public UpdateCounter () {
		stopwatch.Start();
	}

	public void Update () {
		var frame = stopwatch.Elapsed;
		stopwatch.Restart();

		totalTime += frame;
		updateFrames.Enqueue( frame );
		while ( totalTime - updateFrames.Peek() >= MeasuredPeriod ) {
			frame = updateFrames.Dequeue();
			totalTime -= frame;
		}
	}

	public double GetUpdatesPer ( TimeSpan span ) {
		return updateFrames.Count / (totalTime / span);
	}
}
