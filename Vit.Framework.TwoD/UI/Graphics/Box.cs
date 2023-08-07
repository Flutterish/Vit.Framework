using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.Animations;
using Vit.Framework.Graphics.Textures;
using Vit.Framework.Input.Events;
using Vit.Framework.TwoD.Graphics;
using Vit.Framework.TwoD.Input.Events;

namespace Vit.Framework.TwoD.UI.Graphics;

public class Box : Visual<Sprite>, IEventHandler<HoveredEvent> {
	[SetsRequiredMembers]
	public Box () {
		Displayed = new();
	}

	public ColorRgba<float> Tint {
		get => Displayed.Tint;
		set => Displayed.Tint = value;
	}

	public Texture Texture {
		get => Displayed.Texture;
		set => Displayed.Texture = value;
	}

	public bool OnEvent ( HoveredEvent @event ) {
		return true;
	}
}

public static class BoxAnimations {
	class BoxTintAnimation : Animation<Box, ColorRgba<float>> {
		public BoxTintAnimation ( Box target, ColorRgba<float> endValue, double startTime, double endTime, EasingFunction easing ) : base( target, endValue, startTime, endTime, easing ) { }

		protected override ColorRgba<float> GetValue () {
			return Target.Tint;
		}

		protected override void SetValue ( ColorRgba<float> value ) {
			Target.Tint = value;
		}

		protected override ColorRgba<float> Interpolate ( double t ) {
			float time = (float)t;
			return new() {
				R = float.Sqrt( StartValue.R * StartValue.R * (1 - time) + EndValue.R * EndValue.R * time ),
				G = float.Sqrt( StartValue.G * StartValue.G * (1 - time) + EndValue.G * EndValue.G * time ),
				B = float.Sqrt( StartValue.B * StartValue.B * (1 - time) + EndValue.B * EndValue.B * time ),
				A = StartValue.A * (1 - time) + EndValue.A * time
			};
		}

		public static AnimationDomain TintDomain = new() { Name = "Tint" };
		static IReadOnlyList<AnimationDomain> domains = new[] { TintDomain };
		public override IReadOnlyList<AnimationDomain> Domains => domains;
	}
	public static AnimationSequence<T> FadeColour<T> ( this AnimationSequence<T> sequence, ColorRgba<float> tint, double duration, EasingFunction? easing = null ) where T : Box
		=> sequence.Add( new BoxTintAnimation( sequence.Source, tint, sequence.StartTime, sequence.StartTime + duration, easing ?? Easing.None ) );
	public static AnimationSequence<T> FlashColour<T> ( this AnimationSequence<T> sequence, ColorRgba<float> flashTint, ColorRgba<float> tint, double duration, EasingFunction? easing = null ) where T : Box
		=> sequence.FadeColour( flashTint, 0 ).Then().FadeColour( tint, duration, easing );
}