using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

[Flags]
public enum FunctionControl : uint {
	None = 0x0,
	Inline = 0x1,
	DontInline = 0x2,
	Pure = 0x4,
	Const = 0x8,
	OptNoneINTEL = 0x100000
}
