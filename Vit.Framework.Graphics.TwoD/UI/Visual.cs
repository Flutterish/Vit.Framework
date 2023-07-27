using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.TwoD.UI;

public class Visual<T> : UIComponent where T : IDrawable {
	public T? Displayed { get; set; }
}
