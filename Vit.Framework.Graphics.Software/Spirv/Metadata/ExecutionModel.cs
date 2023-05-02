using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public enum ExecutionModel : uint {
	Vertex = 0,
	TesselationControl = 1,
	TesselationEvaluation = 2,
	Geometry = 3,
	Fragment = 4,
	GLCompute = 5,
	Kernel = 6,
	TaskNV = 5267,
	MeshNV = 5268,
	RayGeneration = 5313,
	Intersection = 5314,
	AnyHit = 5315,
	ClosestHit = 5316,
	Miss = 5317,
	Callable = 5318
}
