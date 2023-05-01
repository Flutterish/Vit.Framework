using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Shaders;

public struct ShaderSourceLine {
	public required string OpCode;
	public string Value;
	public uint? Id;
}
