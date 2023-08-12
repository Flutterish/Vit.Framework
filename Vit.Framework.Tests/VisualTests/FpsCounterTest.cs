using Vit.Framework.DependencyInjection;
using Vit.Framework.Performance;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI.Components;

namespace Vit.Framework.Tests.VisualTests;

public class FpsCounterTest : TestScene {
	UpdateCounter counter = new();

	public FpsCounterTest () {
		AddChild( new FpsCounter(), new() {
			Size = new( 1f.Relative() )
		} );
	}

	protected override IReadOnlyDependencyCache CreateDependencies ( IReadOnlyDependencyCache parent ) {
		var cache = new DependencyCache( parent );
		cache.Cache( new FpsCounter.FpsCounterData(
			(" Updates Per Second", counter)
		) );
		return cache;
	}

	public override void Update () {
		counter.Update();
		base.Update();
	}
}
