using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Vit.Framework.Mathematics;

namespace Vit.Framework.Graphics.Animations;

internal abstract class GetterSetterAnimation<TTarget, TValue> : Animation<TTarget, TValue> where TTarget : class {
	public const string WarningMessage = "This is intended for prototyping only. It uses reflections to verify proper usage, compiles code and uses locks to synchronize animation domains. Please create an Animation class and an appropriate extension method instead.";
	static ConcurrentDictionary<MemberInfo, (IReadOnlyList<AnimationDomain> domains, Func<TTarget, TValue> getter)> domains = new();

	Func<TTarget, TValue> getter;
	Action<TTarget, TValue> setter;
	protected GetterSetterAnimation ( TTarget target, Expression<Func<TTarget, TValue>> getter, Action<TTarget, TValue> setter, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) {
		this.setter = setter;

		var body = getter.Body;
		if ( body.NodeType != ExpressionType.MemberAccess || body is not MemberExpression me || me.Expression?.NodeType != ExpressionType.Parameter ) {
			throw new InvalidOperationException( "Getter must be a direct member access ([static] target => target.Member)" );
		}

		(Domains, this.getter) = domains.GetOrAdd( me.Member, static ( member, v ) => {
			return (new[] { new AnimationDomain() { Name = member.Name } }, v.Compile());
		}, getter );
	}

	protected override TValue GetValue () {
		return getter( Target );
	}

	public override void SetValue ( TValue value ) {
		setter( Target, value );
	}

	public override IReadOnlyList<AnimationDomain> Domains { get; }
}

internal class GetterSetterNumberAnimation<TTarget, TValue> : GetterSetterAnimation<TTarget, TValue> where TTarget : class where TValue : INumber<TValue> {
	public GetterSetterNumberAnimation ( TTarget target, Expression<Func<TTarget, TValue>> getter, Action<TTarget, TValue> setter, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( target, getter, setter, endValue, startTime, endTime, easing ) { }

	public override TValue Interpolate ( TValue from, TValue to, double t ) {
		return from.Lerp( to, TValue.CreateSaturating( t ) );
	}
}

internal class GetterSetterInterpolableSingleAnimation<TTarget, TValue> : GetterSetterAnimation<TTarget, TValue> where TTarget : class where TValue : IInterpolatable<TValue, float> {
	public GetterSetterInterpolableSingleAnimation ( TTarget target, Expression<Func<TTarget, TValue>> getter, Action<TTarget, TValue> setter, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( target, getter, setter, endValue, startTime, endTime, easing ) { }

	public override TValue Interpolate ( TValue from, TValue to, double t ) {
		return from.Lerp( to, (float)t );
	}
}

internal class GetterSetterInterpolableDoubleAnimation<TTarget, TValue> : GetterSetterAnimation<TTarget, TValue> where TTarget : class where TValue : IInterpolatable<TValue, double> {
	public GetterSetterInterpolableDoubleAnimation ( TTarget target, Expression<Func<TTarget, TValue>> getter, Action<TTarget, TValue> setter, TValue endValue, double startTime, double endTime, EasingFunction easing ) : base( target, getter, setter, endValue, startTime, endTime, easing ) { }

	public override TValue Interpolate ( TValue from, TValue to, double t ) {
		return from.Lerp( to, t );
	}
}

public static class AnimationExtensionsA {
	[Obsolete( GetterSetterAnimation<object, float>.WarningMessage )]
	public static AnimationSequence<T> Mutate<T, TValue> ( this AnimationSequence<T> sequence, Expression<Func<T, TValue>> getter, Action<T, TValue> setter, TValue endValue, double duration, EasingFunction? easing = null )
		where T : class, ICanBeAnimated
		where TValue : INumber<TValue>
		=> sequence.Add( new GetterSetterNumberAnimation<T, TValue>( sequence.Source, getter, setter, endValue, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
}

public static class AnimationExtensionsB {
	[Obsolete( GetterSetterAnimation<object, float>.WarningMessage )]
	public static AnimationSequence<T> Mutate<T, TValue> ( this AnimationSequence<T> sequence, Expression<Func<T, TValue>> getter, Action<T, TValue> setter, TValue endValue, double duration, EasingFunction? easing = null )
		where T : class, ICanBeAnimated
		where TValue : IInterpolatable<TValue, float>
		=> sequence.Add( new GetterSetterInterpolableSingleAnimation<T, TValue>( sequence.Source, getter, setter, endValue, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
}

public static class AnimationExtensionsC {
	[Obsolete( GetterSetterAnimation<object, float>.WarningMessage )]
	public static AnimationSequence<T> Mutate<T, TValue> ( this AnimationSequence<T> sequence, Expression<Func<T, TValue>> getter, Action<T, TValue> setter, TValue endValue, double duration, EasingFunction? easing = null )
		where T : class, ICanBeAnimated
		where TValue : IInterpolatable<TValue, double>
		=> sequence.Add( new GetterSetterInterpolableDoubleAnimation<T, TValue>( sequence.Source, getter, setter, endValue, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
}