using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct Fixed16_16 {
	public const int Divisor = 1 << 16;
	public int Data;

	public override string ToString () {
		return $"{(double)Data / Divisor}";
	}
}
