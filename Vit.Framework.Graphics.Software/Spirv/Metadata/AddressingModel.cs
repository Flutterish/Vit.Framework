using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Metadata;

public enum AddressingModel : uint {
	Logical = 0,
	Physical32 = 1,
	Physical64 = 2,
	PhysicalStorageBuffer64 = 5348
}
