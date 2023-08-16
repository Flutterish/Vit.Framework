using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Animations;

public interface IValueInterpolatingAnimation<TValue> {
	void SetValue ( TValue value );
	TValue GetValue ( Millis time );
	void ChangeInterpolatedStartValue ( TValue value );
}

public class ValueInterpolatingAnimationOverlapResolutionContract<TValue> : OverlapResolutionContract {
	public ValueInterpolatingAnimationOverlapResolutionContract () { }
	public static readonly ValueInterpolatingAnimationOverlapResolutionContract<TValue> Value = new();

	[ThreadStatic]
	static List<IValueInterpolatingAnimation<TValue>>? _considered;
	static List<IValueInterpolatingAnimation<TValue>> getConsidered ( IEnumerable<Animation> animations ) {
		var considered = _considered ??= new();

		Animation? previous = null;
		foreach ( var i in animations ) {
			var value = (IValueInterpolatingAnimation<TValue>)i;

			if ( previous != null && i.InterruptedBy != previous ) {
				break;
			}

			previous = i;
			considered.Add( value );
		}

		return considered;
	}

	public override void Update ( IEnumerable<Animation> animations, Millis time ) {
		var considered = getConsidered( animations );

		IValueInterpolatingAnimation<TValue>? previous = considered[^1];
		for ( int i = considered.Count - 2; i >= 0; i-- ) {
			var current = considered[i];
			current.ChangeInterpolatedStartValue( previous.GetValue( time ) );

			previous = current;
		}

		considered.Clear();
		previous.SetValue( previous.GetValue( time ) );
	}
}
