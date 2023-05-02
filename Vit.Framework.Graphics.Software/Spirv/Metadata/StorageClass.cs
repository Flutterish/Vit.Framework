using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public enum StorageClass : uint {
	UniformConstant = 0,
	Input = 1,
	Uniform = 2,
	Output = 3,
	Workgroup = 4,
	CrossWorkgroup = 5,
	Private = 6,
	Function = 7,
	Generic = 8,
	PushConstant = 9,
	AtomicCounter = 10,
	Image = 11,
	StorageBuffer = 12,
	CallableData = 5328,
	IncomingCallableData = 5329,
	RayPayLoad = 5338,
	HitAttribute = 5339,
	IncomingRayPayload = 5342,
	ShaderRecordBuffer = 5343,
	PhysicalStorageBuffer = 5349,
	CodeSectionINTEL = 5605,
	DeviceOnlyINTEL = 5936,
	HostOnlyINTEL = 5937
}
