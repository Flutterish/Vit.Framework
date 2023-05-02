using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv;

public struct SpirvInstruction {
	public OpCode OpCode;
	public ushort WordCount;

	public override string ToString () {
		return $"Op{OpCode} +{WordCount}";
	}
}
