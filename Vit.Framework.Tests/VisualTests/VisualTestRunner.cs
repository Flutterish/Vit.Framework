using System.Diagnostics.CodeAnalysis;
using Vit.Framework.Graphics;
using Vit.Framework.Graphics.TwoD;
using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;
using Vit.Framework.Graphics.TwoD.Text;
using Vit.Framework.Parsing;
using Vit.Framework.Text.Fonts.OpenType;

namespace Vit.Framework.Tests.VisualTests;

public class VisualTestRunner : Flexbox {
	LayoutContainer testArea;
	FlowContainer sidebar;

	[SetsRequiredMembers]
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

		var font = new OpenTypeFont( new ReopenableFileStream( "./CONSOLA.TTF" ) );
		foreach ( var i in tests ) {
			var button = new LayoutContainer();
			button.AddChild( new Sprite { Tint = ColorRgba.GreenYellow }, new() {
				Size = new( 1f.Relative() )
			} );
			button.AddChild( new SpriteText { 
				Tint = ColorRgba.Black,
				Font = font,
				FontSize = 32,
				Text = $"{i.Name}"
			}, new() {
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
		testArea.ClearChildren();
		testArea.AddChild( (ILayoutElement)Activator.CreateInstance(type)!, new() {
			Size = new( 1f.Relative(), 1f.Relative() )
		} );
	}
}
