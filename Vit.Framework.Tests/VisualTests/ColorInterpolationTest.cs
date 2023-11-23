using Vit.Framework.Graphics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.Tests.VisualTests;

public class ColorInterpolationTest : TestScene {
	public ColorInterpolationTest () {
		var red = new ColorRgb<float>( 1f, 0, 0 );
		var green = new ColorRgb<float>( 0, 1f, 0 );
		var blue = new ColorRgb<float>( 0, 0, 1f );

		var antiRed = new ColorRgb<float>( 0, 1f, 1f );
		var antiGreen = new ColorRgb<float>( 1f, 0, 1f );
		var antiBlue = new ColorRgb<float>( 1f, 1f, 0 );

		var tests = new (ColorRgb<float> start, ColorRgb<float> end)[] {
			(ColorRgb.White, ColorRgb.Black),
			(red, blue),
			(blue, green),
			(green, red),
			(antiRed, antiBlue),
			(antiBlue, antiGreen),
			(antiGreen, antiRed),
			(ColorRgb.White, red),
			(ColorRgb.White, green),
			(ColorRgb.White, blue),
			(ColorRgb.Black, red),
			(ColorRgb.Black, green),
			(ColorRgb.Black, blue)
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
