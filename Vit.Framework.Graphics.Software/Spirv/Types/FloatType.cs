using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class FloatType : DataType {
	public FloatType ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint Width;

	public override object? Parse ( ReadOnlySpan<byte> data ) {
		if ( Width == 32 ) {
			return BitConverter.ToSingle( data );
		}

		return base.Parse( data );
	}

	public override string ToString () {
		return $"f{Width}";
	}
}
