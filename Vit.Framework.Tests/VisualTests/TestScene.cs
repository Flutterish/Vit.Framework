﻿using Vit.Framework.TwoD.UI;
using Vit.Framework.TwoD.UI.Layout;

namespace Vit.Framework.Tests.VisualTests;

public abstract class TestScene : LayoutContainer<UIComponent> {
	public TestScene () {
		Padding = new(10);
	}
}
