using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.Tests.VisualTests;

public class ColorInterpolationTest : TestScene {
	public ColorInterpolationTest () {
		var red = new ColorRgba<float>( 1f, 0, 0, 1f );
		var green = new ColorRgba<float>( 0, 1f, 0, 1f );
		var blue = new ColorRgba<float>( 0, 0, 1f, 1f );

		var antiRed = new ColorRgba<float>( 0, 1f, 1f, 1f );
		var antiGreen = new ColorRgba<float>( 1f, 0, 1f, 1f );
		var antiBlue = new ColorRgba<float>( 1f, 1f, 0, 1f );

		var tests = new (ColorRgba<float> start, ColorRgba<float> end)[] {
			(ColorRgba.White, ColorRgba.Black),
			(red, blue),
			(blue, green),
			(green, red),
			(antiRed, antiBlue),
			(antiBlue, antiGreen),
			(antiGreen, antiRed),
			(ColorRgba.White, red),
			(ColorRgba.White, green),
			(ColorRgba.White, blue),
			(ColorRgba.Black, red),
			(ColorRgba.Black, green),
			(ColorRgba.Black, blue)
		};

		var steps = 50;
		for ( int i = 0; i < steps; i++ ) {
			var t = 1f / (steps - 1) * i;
			var x = 1f / steps * i;

			for ( int j = 0; j < tests.Length; j++ ) {
				var (start, end) = tests[j];
				var y = 1f / tests.Length * j;

				AddChild( new Box { Tint = start.Interpolate( end, t ) }, new() {
					Size = new( (1f / steps).Relative(), (1f / tests.Length).Relative() ),
					Origin = Anchor.TopLeft,
					Anchor = Anchor.TopLeft + new RelativeAxes2<float>( x.Relative(), -y.Relative() )
				} );
			}		
		}
	}
}
