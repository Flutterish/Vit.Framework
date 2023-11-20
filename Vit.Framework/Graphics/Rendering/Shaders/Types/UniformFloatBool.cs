using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Rendering.Shaders.Types;

public struct UniformFloatBool {
	public float Value;
	
	public static implicit operator UniformFloatBool ( bool value ) {
		return new() { Value = value ? 1 : 0 };
	}
}
