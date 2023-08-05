using System.Diagnostics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.TwoD.UI.Components;

public class FpsCounter : LayoutContainer {
	SpriteText text;

	public FpsCounter () {
		AddChild( text = new() { FontSize = 32 }, new() {
			Size = new( 1f.Relative() )
		} );

		stopwatch.Start();
	}

	public TimeSpan MeasuredPeriod = TimeSpan.FromSeconds( 1 );

	TimeSpan totalTime;
	Queue<TimeSpan> frames = new();
	Stopwatch stopwatch = new();
	public override void Update () {
		var frame = stopwatch.Elapsed;
		stopwatch.Restart();

		totalTime += frame;
		frames.Enqueue( frame );
		while ( totalTime - frames.Peek() >= MeasuredPeriod ) {
			frame = frames.Dequeue();
			totalTime -= frame;
		}

		text.Text = $"{frames.Count/totalTime.TotalSeconds:N1} TPS";

		base.Update();
	}
}
