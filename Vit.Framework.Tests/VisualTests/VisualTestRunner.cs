using System.Reflection;
using Vit.Framework.DependencyInjection;
using Vit.Framework.Mathematics;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.Rendering;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Input;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class VisualTestRunner : Flexbox {
	LayoutContainer testArea;
	FlowContainer sidebar;

	TypeInfo[] tests;
	public VisualTestRunner () {
		ContentAlignment = Anchor.TopLeft;
		FlowDirection = FlowDirection.Right;
		LineJustification = LineJustification.Stretch;

		tests = typeof(VisualTestRunner).Assembly.DefinedTypes.Where( x => !x.IsAbstract && x.IsAssignableTo( typeof(TestScene) ) ).ToArray();

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
			var button = new BasicButton() {
				Clicked = () => runTest( i ),
				RawText = i.Name,
				TextAnchor = Anchor<float>.CentreLeft + new Vector2<float>(10, 0)
			};
			sidebar.AddChild( button, new() {
				Size = new( 1f.Relative(), 80 ),
				Margins = new() { Vertical = 10 }
			} );
		}

	}

	RenderThreadScheduler RenderThreadScheduler = null!;
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		RenderThreadScheduler = dependencies.Resolve<RenderThreadScheduler>();
		base.OnLoad( dependencies );

		runTest( tests[0] );
	}

	void runTest ( Type type ) {
		testArea.DisposeChildren( RenderThreadScheduler );
		var instance = Activator.CreateInstance( type )!;

		testArea.AddChild( (UIComponent)instance, new() {
			Size = new( 1f.Relative(), 1f.Relative() )
		} );
	}
}
