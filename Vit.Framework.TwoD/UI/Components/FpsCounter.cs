using Vit.Framework.DependencyInjection;
using Vit.Framework.Performance;
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
	}

	FpsCounterData data = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		data = dependencies.Resolve<FpsCounterData>();
	}

	public override void Update () {
		text.Text = string.Join( " / ", data.Counters.Select( x => $"{x.counter.GetUpdatesPer(TimeSpan.FromSeconds(1)):N1}{x.tickName}" ) );

		base.Update();
	}

	public class FpsCounterData {
		public FpsCounterData ( params (string tickName, UpdateCounter counter)[] counters ) {
			Counters = counters;
		}

		public readonly (string tickName, UpdateCounter counter)[] Counters;
	}
}
