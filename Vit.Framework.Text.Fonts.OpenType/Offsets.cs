using System.Diagnostics;

namespace Vit.Framework.Text.Fonts.OpenType;

[DebuggerDisplay("0x{Value.ToString(\"x\"),nq}")]
public struct Offset32 {
	public uint Value;

	public static implicit operator long ( Offset32 offset )
		=> offset.Value;
}
