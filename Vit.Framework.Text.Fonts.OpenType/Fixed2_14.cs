namespace Vit.Framework.Text.Fonts.OpenType;

public struct Fixed2_14 {
	public const short Divisor = 1 << 14;
	public short Data;

	public static Fixed2_14 Zero => new() { Data = 0 };
	public static Fixed2_14 One => new() { Data = Divisor };

	public static bool operator == ( Fixed2_14 left, Fixed2_14 right )
		=> left.Data == right.Data;
	public static bool operator != ( Fixed2_14 left, Fixed2_14 right )
		=> left.Data != right.Data;

	public override string ToString () {
		return $"{(double)Data / Divisor}";
	}
}
