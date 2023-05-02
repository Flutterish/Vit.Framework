using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class IntType : DataType {
	public IntType ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint Width;
	public bool Signed;

	public override object? Parse ( ReadOnlySpan<byte> data ) {
		if ( Width == 32 ) {
			return Signed ? BitConverter.ToInt32( data ) : BitConverter.ToUInt32( data );
		}

		return base.Parse( data );
	}

	public override string ToString () {
		return $"{(Signed ? "i" : "u")}{Width}";
	}
}
