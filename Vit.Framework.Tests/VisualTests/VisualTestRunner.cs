using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Graphics;
using Vit.Framework.TwoD.UI.Input;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class VisualTestRunner : Flexbox {
	LayoutContainer testArea;
	FlowContainer sidebar;

	public VisualTestRunner () {
		ContentAlignment = Anchor.TopLeft;
		FlowDirection = FlowDirection.Right;
		LineJustification = LineJustification.Stretch;

		var tests = typeof(VisualTestRunner).Assembly.DefinedTypes.Where( x => !x.IsAbstract && x.IsAssignableTo( typeof(TestScene) ) ).ToArray();

		AddChild( sidebar = new FlowContainer {
			ContentAlignment = Anchor.TopLeft,
			FlowDirection = FlowDirection.Down,
			LineJustification = LineJustification.Stretch
		}, new() {
			Size = new( 500, 1f.Relative() )
		} );

		AddChild( testArea = new(), new() {
			Size = new( 0, 1f.Relative() ),
			Grow = 1
		} );

		foreach ( var i in tests ) {
			var button = new Button() {
				Clicked = () => {
					runTest( i );
				}
			};
			button.AddChild( new SpriteText( i.Name ), new() {
				Size = new( 1f.Relative(), 32 ),
				Anchor = new( 10, 0.5f.Relative() ),
				Origin = Anchor.CentreLeft
			} );
			sidebar.AddChild( button, new() {
				Size = new( 1f.Relative(), 80 ),
				Margins = new() { Vertical = 10 }
			} );
		}

		runTest( tests[0] );
	}

	void runTest ( Type type ) {
		testArea.ClearChildren( dispose: true );
		var instance = Activator.CreateInstance( type )!;

		testArea.AddChild( (UIComponent)instance, new() {
			Size = new( 1f.Relative(), 1f.Relative() )
		} );
	}
}
