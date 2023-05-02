using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class IntType : DataType {
	public IntType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint Width;
	public bool Signed;

	public override object? Parse ( ReadOnlySpan<byte> data ) {
		if ( Width == 32 ) {
			return Signed ? BitConverter.ToInt32( data ) : BitConverter.ToUInt32( data );
		}

		return base.Parse( data );
	}

	protected override IRuntimeType CreateRuntimeType () {
		if ( Width == 32 ) {
			return Signed ? new RuntimeNumberType<int>() : new RuntimeNumberType<uint>();
		}
		
		return base.CreateRuntimeType();
	}

	public override string ToString () {
		return $"{(Signed ? "i" : "u")}{Width}";
	}
}
