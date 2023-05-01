using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Shaders.Spirv;

public struct Version {
	private byte pad0;
	public byte Minor;
	public byte Major;
	private byte pad1;
}
