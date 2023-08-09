using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.Tests.VisualTests;

public class ColorInterpolationTest : TestScene {
	public ColorInterpolationTest () {
		var green = new ColorRgba<float>( 0, 1f, 0, 1f );
		var tests = new (ColorRgba<float> start, ColorRgba<float> end)[] {
			(ColorRgba.White, ColorRgba.Black),
			(ColorRgba.Red, ColorRgba.Blue),
			(ColorRgba.Blue, green),
			(green, ColorRgba.Red),
			(ColorRgba.White, ColorRgba.Red),
			(ColorRgba.White, green),
			(ColorRgba.White, ColorRgba.Blue),
			(ColorRgba.Black, ColorRgba.Red),
			(ColorRgba.Black, green),
			(ColorRgba.Black, ColorRgba.Blue)
		};

		var steps = 50;
		for ( int i = 0; i < steps; i++ ) {
			var t = 1f / (steps - 1) * i;
			var x = 1f / steps * i;

			for ( int j = 0; j < tests.Length; j++ ) {
				var (start, end) = tests[j];
				var y = 1f / tests.Length * j;

				AddChild( new Box { Tint = start.InterpolateSubtractive( end, t ) }, new() {
					Size = new( (1f / steps).Relative(), (1f / tests.Length).Relative() ),
					Origin = Anchor.TopLeft,
					Anchor = Anchor.TopLeft + new RelativeAxes2<float>( x.Relative(), -y.Relative() )
				} );
			}		
		}
	}
}
