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
