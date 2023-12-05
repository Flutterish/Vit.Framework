using Vit.Framework.DependencyInjection;
using Vit.Framework.Graphics.Rendering;
using Vit.Framework.TwoD.Layout;
using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Composite;
using Vit.Framework.TwoD.UI.Input;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.VisualTests;

public class SwitchBackendsTest : TestScene {
	protected override void OnLoad ( IReadOnlyDependencyCache dependencies ) {
		base.OnLoad( dependencies );

		var app = dependencies.Resolve<TwoDTestApp>();
		AddChild( new Flexbox {
			FlowDirection = FlowDirection.RightThenDown,
			Gap = (10, 10),
			LayoutChildrenEnumerable = new[] { KnownGraphicsApiName.OpenGl, KnownGraphicsApiName.Vulkan, KnownGraphicsApiName.Direct3D11 }.Select( 
				type => (ParametrizedChildData<UIComponent, FlexboxParams>)((UIComponent)new BasicButton { 
					RawText = type.ToString(),
					TextAnchor = Anchor.Centre,
					TextOrigin = Anchor.Centre,
					Clicked = () => {
						app.SwitchBackends( type );
					}
				}, new FlexboxParams {
					Size = new(300, 100)
				})
			)
		}, new() {
			Size = new(1f.Relative(), 1f.Relative())
		} );
	}
}
