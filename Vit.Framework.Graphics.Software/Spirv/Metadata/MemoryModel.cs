using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public enum MemoryModel : uint {
	Simple = 0,
	GLSL450 = 1,
	OpenCL = 2,
	Vulkan = 3
}
