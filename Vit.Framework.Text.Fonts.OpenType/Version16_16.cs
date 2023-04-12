using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct Version16_16 {
	public ushort Major;
	public ushort MinorData;
	public ushort Minor => (ushort)(MinorData >> 12);

	public override string ToString () {
		return $"{Major}.{Minor}";
	}
}
