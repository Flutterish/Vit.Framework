namespace Vit.Framework.Text.Fonts.OpenType;

public struct LongDateTime {
	public long Data;

	public override string ToString () {
		return (new DateTime( 1904, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc ) + TimeSpan.FromSeconds( Data )).ToString();
	}
}
