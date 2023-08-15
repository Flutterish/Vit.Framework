namespace Vit.Framework.Graphics.Animations;

public delegate double EasingFunction ( double t );

/// <summary>
/// See http://easings.net/ for samples.
/// </summary>
public static class Easing {
	public static readonly EasingFunction InSine = x => { return 1 - double.Cos((x * double.Pi) / 2); };
	public static readonly EasingFunction OutSine = x => { return double.Sin( (x * double.Pi) / 2 ); };
	public static readonly EasingFunction InOutSine = x => { return -(double.Cos( double.Pi * x ) - 1) / 2; };
	public static readonly EasingFunction InQuad = x => { return x * x; };
	public static readonly EasingFunction OutQuad = x => { return 1 - (1 - x) * (1 - x); };
	public static readonly EasingFunction InOutQuad = x => { return x < 0.5 ? 2 * x * x : 1 - double.Pow( -2 * x + 2, 2 ) / 2; };
	public static readonly EasingFunction InCubic = x => { return x * x * x; };
	public static readonly EasingFunction OutCubic = x => { return 1 - double.Pow( 1 - x, 3 ); };
	public static readonly EasingFunction InOutCubic = x => { return x < 0.5 ? 4 * x * x * x : 1 - double.Pow( -2 * x + 2, 3 ) / 2; };
	public static readonly EasingFunction InQuart = x => { return x * x * x * x; };
	public static readonly EasingFunction OutQuart = x => { return 1 - double.Pow( 1 - x, 4 ); };
	public static readonly EasingFunction InOutQuart = x => { return x < 0.5 ? 8 * x * x * x * x : 1 - double.Pow( -2 * x + 2, 4 ) / 2; };
	public static readonly EasingFunction InQuint = x => { return x * x * x * x * x; };
	public static readonly EasingFunction OutQuint = x => { return 1 - double.Pow( 1 - x, 5 ); };
	public static readonly EasingFunction InOutQuint = x => { return x < 0.5 ? 16 * x * x * x * x * x : 1 - double.Pow( -2 * x + 2, 5 ) / 2; };
	public static readonly EasingFunction InExpo = x => { return x == 0 ? 0 : double.Pow( 2, 10 * x - 10 ); };
	public static readonly EasingFunction OutExpo = x => { return x == 1 ? 1 : 1 - double.Pow( 2, -10 * x ); };
	public static readonly EasingFunction InOutExpo = x => {
		return x == 0
		  ? 0
		  : x == 1
		  ? 1
		  : x < 0.5 ? double.Pow( 2, 20 * x - 10 ) / 2
		  : (2 - double.Pow( 2, -20 * x + 10 )) / 2;
	};
	public static readonly EasingFunction InCirc = x => { return 1 - double.Sqrt( 1 - double.Pow( x, 2 ) ); };
	public static readonly EasingFunction OutCirc = x => { return double.Sqrt( 1 - double.Pow( x - 1, 2 ) ); };
	public static readonly EasingFunction InOutCirc = x => {
		return x < 0.5
	  ? (1 - double.Sqrt( 1 - double.Pow( 2 * x, 2 ) )) / 2
	  : (double.Sqrt( 1 - double.Pow( -2 * x + 2, 2 ) ) + 1) / 2;
	};
	public static readonly EasingFunction InBack = x => {
		const double c1 = 1.70158;
		const double c3 = c1 + 1;

		return c3 * x * x * x - c1 * x * x;
	};
	public static readonly EasingFunction OutBack = x => {
		const double c1 = 1.70158;
		const double c3 = c1 + 1;

		return 1 + c3 * double.Pow( x - 1, 3 ) + c1 * double.Pow( x - 1, 2 );
	};
	public static readonly EasingFunction InOutBack = x => {
		const double c1 = 1.70158;
		const double c2 = c1 * 1.525;

		return x < 0.5
		  ? (double.Pow( 2 * x, 2 ) * ((c2 + 1) * 2 * x - c2)) / 2
		  : (double.Pow( 2 * x - 2, 2 ) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
	};
	public static readonly EasingFunction InElastic = x => {
		const double c4 = (2 * double.Pi) / 3;

		return x == 0
		  ? 0
		  : x == 1
		  ? 1
		  : -double.Pow( 2, 10 * x - 10 ) * double.Sin( (x * 10 - 10.75) * c4 );
	};
	public static readonly EasingFunction OutElastic = x => {
		const double c4 = (2 * double.Pi) / 3;

		return x == 0
		  ? 0
		  : x == 1
		  ? 1
		  : double.Pow( 2, -10 * x ) * double.Sin( (x * 10 - 0.75) * c4 ) + 1;
	};
	public static readonly EasingFunction InOutElastic = x => {
		const double c5 = (2 * double.Pi) / 4.5;

		return x == 0
		  ? 0
		  : x == 1
		  ? 1
		  : x < 0.5
		  ? -(double.Pow( 2, 20 * x - 10 ) * double.Sin( (20 * x - 11.125) * c5 )) / 2
		  : (double.Pow( 2, -20 * x + 10 ) * double.Sin( (20 * x - 11.125) * c5 )) / 2 + 1;
	};
	public static readonly EasingFunction OutBounce = x => {
		const double n1 = 7.5625;
		const double d1 = 2.75;

		if ( x < 1 / d1 ) {
			return n1 * x * x;
		}
		else if ( x < 2 / d1 ) {
			return n1 * (x -= 1.5 / d1) * x + 0.75;
		}
		else if ( x < 2.5 / d1 ) {
			return n1 * (x -= 2.25 / d1) * x + 0.9375;
		}
		else {
			return n1 * (x -= 2.625 / d1) * x + 0.984375;
		}
	};
	public static readonly EasingFunction InOutBounce = x => {
		return x < 0.5
		  ? (1 - OutBounce( 1 - 2 * x )) / 2
		  : (1 + OutBounce( 2 * x - 1 )) / 2;
	};
	public static readonly EasingFunction InBounce = x => { return 1 - OutBounce( 1 - x ); };

	public static readonly EasingFunction SmoothStep = x => x * x * ( 3 - 2 * x );
	public static readonly EasingFunction SmootherStep = x => x * x * x * ( 10 - x * ( 15 - x * 6 ) );
	public static readonly EasingFunction SmoothestStep = x => x * x * x * x * ( 35 - x * ( 84 - x * ( 70 - x * 20 ) ) );

	public static readonly EasingFunction None = x => x;
	public static readonly EasingFunction In = InQuad;
	public static readonly EasingFunction InOut = InOutQuad;
	public static readonly EasingFunction Out = OutQuad;
}
