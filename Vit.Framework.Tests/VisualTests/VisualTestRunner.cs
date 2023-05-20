using Vit.Framework.Graphics.TwoD.Containers;
using Vit.Framework.Graphics.TwoD.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class VisualTestRunner : FlowContainer<ILayoutElement> {
	public VisualTestRunner () {
		var tests = typeof(VisualTestRunner).Assembly.DefinedTypes.Where( x => !x.IsAbstract && x.IsAssignableTo( typeof(TestScene) ) ).ToArray();

		runTest( tests[0] );
	}

	void runTest ( Type type ) {
		AddChild( (ILayoutElement)Activator.CreateInstance(type)!, new() {
			Size = new( 1f.Relative(), 1f.Relative() )
		} );
	}
}
