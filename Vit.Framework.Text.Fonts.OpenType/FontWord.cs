using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct FontWord {
	public short Data;

	public override string ToString () {
		return $"{Data} Units";
	}
}

public struct UFontWord {
	public ushort Data;

	public override string ToString () {
		return $"{Data} Units";
	}
}
