using Vit.Framework.TwoD.UI;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

/// <summary>
/// A debug screen overlay which allows inspecting the draw hierarchy. Includes ability to visually edit components via "blueprints".
/// </summary>
public class DrawVisualizer : CompositeUIComponent {
	DrawHierarchyVisualizer hierarchy;
	DrawVisualizerCursor cursor;

	public DrawVisualizer () {
		AddInternalChild( hierarchy = new DrawHierarchyVisualizer {
			Size = (800, 1000)
		} );
		AddInternalChild( cursor = new() );

		hierarchy.Selected = t => cursor.Target = t;
	}

	public void View ( IViewableInDrawVisualiser? target ) {
		hierarchy.View( target );
	}
}
