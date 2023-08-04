using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	int frames;
	Stopwatch stopwatch = new();
	public override void Update () {
		frames++;
		var seconds = stopwatch.Elapsed.TotalSeconds;
		if ( seconds >= 1 ) {
			text.Text = $"{frames/seconds:N0} FPS";
			frames = 0;
			stopwatch.Restart();
		}

		base.Update();
	}
}
