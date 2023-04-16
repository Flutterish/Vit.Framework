namespace Vit.Framework.Text.Fonts.OpenType;

public struct Tag {
	public byte A;
	public byte B;
	public byte C;
	public byte D;

	public Tag ( string name ) {
		A = (byte)name[0];
		B = (byte)name[1];
		C = (byte)name[2];
		D = (byte)name[3];
	}

	public static implicit operator Tag ( string str ) => new( str );

	public static bool operator == ( Tag tag, string str ) {
		return str.Length == 4
			&& (char)tag.A == str[0]
			&& (char)tag.B == str[1]
			&& (char)tag.C == str[2]
			&& (char)tag.D == str[3];
	}
	public static bool operator != ( Tag tag, string str ) => !(tag == str);

	public static bool operator == ( Tag left, Tag right ) {
		return left.A == right.A
			&& left.B == right.B
			&& left.C == right.C
			&& left.D == right.D;
	}
	public static bool operator != ( Tag left, Tag right ) => !(left == right);

	public override string ToString () {
		return $"{(char)A}{(char)B}{(char)C}{(char)D}";
	}

	public override bool Equals ( object? obj ) {
		return obj is Tag tag &&
				 A == tag.A &&
				 B == tag.B &&
				 C == tag.C &&
				 D == tag.D;
	}

	public override int GetHashCode () {
		return HashCode.Combine( A, B, C, D );
	}
}
