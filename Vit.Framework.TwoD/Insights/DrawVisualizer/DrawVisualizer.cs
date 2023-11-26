using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;

namespace Vit.Framework.TwoD.Insights.DrawVisualizer;

/// <summary>
/// A debug screen overlay which allows inspecting the draw hierarchy. Includes ability to visually edit components via "blueprints".
/// </summary>
public class DrawVisualizer : CompositeUIComponent {
	DrawHierarchyVisualizer hierarchy;
	DraggableContainer container;
	DrawVisualizerCursor cursor;

	public DrawVisualizer () {
		AddInternalChild( cursor = new() );
		AddInternalChild( container = new() {
			Size = (800, 1000)
		} );
		container.Content.AddChild( hierarchy = new(), new() {
			Size = new( 1f.Relative() )
		} );

		hierarchy.Selected = t => cursor.Target = t;
	}

	public void View ( IViewableInDrawVisualiser? target ) {
		hierarchy.View( target );
	}
}
