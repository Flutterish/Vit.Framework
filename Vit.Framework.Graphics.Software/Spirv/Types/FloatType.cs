using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class FloatType : DataType {
	public FloatType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint Width;

	public override object? Parse ( ReadOnlySpan<byte> data ) {
		if ( Width == 32 ) {
			return BitConverter.ToSingle( data );
		}

		return base.Parse( data );
	}


	protected override IRuntimeType CreateRuntimeType () {
		if ( Width == 32 ) {
			return new RuntimeNumberType<float>();
		}

		return base.CreateRuntimeType();
	}

	public override string ToString () {
		return $"f{Width}";
	}
}
