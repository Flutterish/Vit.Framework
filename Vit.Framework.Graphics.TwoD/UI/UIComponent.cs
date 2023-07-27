﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.TwoD.UI;

public class UIComponent {
	public static implicit operator UIComponent ( Drawable drawable )
		=> new Visual<Drawable> { Displayed = drawable };
}
