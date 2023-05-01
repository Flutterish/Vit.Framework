using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Shaders.Spirv;

public enum ExecutionModel : uint {
	Vertex = 0,
	Fragment = 4
}

public enum ExecutionMode : uint {
	Invocations = 0,
	OriginUpperLeft = 7
}