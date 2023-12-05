using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI.Composite;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

public class BlueprintContainer : InternalContainer<DrawVisualizerBlueprint> {
	DrawVisualizerBlueprint? blueprint;
	public DrawVisualizerBlueprint? Blueprint {
		get => blueprint;
		set {
			if ( blueprint == value )
				return;

			if ( blueprint != null ) {
				RemoveInternalChild( blueprint );
				blueprint.Dispose( disposeScheduler );
			}

			blueprint = value;
			if ( blueprint != null ) {
				AddInternalChild( blueprint );
			}
		}
	}
	public IViewableInDrawVisualiser? Target;

	RenderThreadScheduler disposeScheduler = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );
		disposeScheduler = dependencies.Resolve<RenderThreadScheduler>();
	}

	protected override void PerformSelfLayout () {
		if ( Blueprint == null ) {
			return;
		}
		if ( Target == null ) {
			Blueprint.Scale = Axes2<float>.Zero;
			return;
		}

		Blueprint.Scale = Axes2<float>.One;
		var a = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 0, 0 ) ) );
		var b = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 1, 1 ) ) );
		var c = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 1, 0 ) ) );
		var d = ScreenSpaceToLocalSpace( Target.UnitToGlobalMatrix.Apply( new Point2<float>( 0, 1 ) ) );

		AxisAlignedBox2<float> box = new() {
			MinX = float.Min( float.Min( a.X, b.X ), float.Min( c.X, d.X ) ),
			MaxX = float.Max( float.Max( a.X, b.X ), float.Max( c.X, d.X ) ),
			MinY = float.Min( float.Min( a.Y, b.Y ), float.Min( c.Y, d.Y ) ),
			MaxY = float.Max( float.Max( a.Y, b.Y ), float.Max( c.Y, d.Y ) ),
		};

		Blueprint.Position = box.Position;
		Blueprint.Size = box.Size;
	}

	public override void Update () {
		base.Update();
		InvalidateLayout( LayoutInvalidations.Self );
	}
}
