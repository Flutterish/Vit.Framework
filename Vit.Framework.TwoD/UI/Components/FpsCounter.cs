using Vit.Framework.DependencyInjection;
using Vit.Framework.Performance;
using Vit.Framework.TwoD.UI.Graphics;

namespace Vit.Framework.TwoD.UI.Components;

public class FpsCounter : SpriteText {
	public FpsCounter () {
		FontSize = 32;
	}

	FpsCounterData data = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		data = dependencies.Resolve<FpsCounterData>();
	}

	public override void Update () {
		// TODO this can definitely be localised, but its too much effort right now and it spams the store
		RawText = string.Join( " / ", data.Counters.Select( x => $"{x.counter.GetUpdatesPer(TimeSpan.FromSeconds(1)):N1}{x.tickName}" ) );

		base.Update();
	}

	public class FpsCounterData {
		public FpsCounterData ( params (string tickName, UpdateCounter counter)[] counters ) {
			Counters = counters;
		}

		public readonly (string tickName, UpdateCounter counter)[] Counters;
	}
}
