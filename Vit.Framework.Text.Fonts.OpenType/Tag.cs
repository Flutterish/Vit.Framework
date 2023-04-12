using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Text.Fonts.OpenType;

public struct Tag {
	public byte A;
	public byte B;
	public byte C;
	public byte D;

	public static bool operator == ( Tag tag, string str ) {
		return str.Length == 4
			&& (char)tag.A == str[0]
			&& (char)tag.B == str[1]
			&& (char)tag.C == str[2]
			&& (char)tag.D == str[3];
	}
	public static bool operator != ( Tag tag, string str ) => !(tag == str);

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
