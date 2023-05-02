using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Constant : CompilerObject, IValue {
	public Constant ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint DataTypeId;
	public uint[] Data = Array.Empty<uint>();

	public DataType Type => GetDataType( DataTypeId );

	public override string ToString () {
		var data = MemoryMarshal.Cast<uint, byte>( Data.AsSpan() );
		return $"const {GetDataType( DataTypeId )} = {GetDataType( DataTypeId )?.Parse( data ) ?? $"0x{Convert.ToHexString( data )}"}";
	}
}

public class ConstantComposite : CompilerObject, IValue {
	public ConstantComposite ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint DataTypeId;
	public uint[] ValueIds = Array.Empty<uint>();

	public override string ToString () {
		var values = ValueIds.Select( GetValue );
		return $"const {GetDataType( DataTypeId )} = {{{string.Join(", ", values)}}}";
	}
}
