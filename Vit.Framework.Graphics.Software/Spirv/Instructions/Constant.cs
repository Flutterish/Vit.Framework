using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Constant : CompilerObject, IValue {
	public Constant ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint DataTypeId;
	public uint[] Data = Array.Empty<uint>();

	public DataType Type => GetDataType( DataTypeId );

	public IVariable CreateVariable () {
		var type = Type.GetRuntimeType();
		var variable = type.CreateVariable();
		variable.Parse( MemoryMarshal.AsBytes( Data.AsSpan() ) );
		return variable;
	}

	public override string ToString () {
		var data = MemoryMarshal.Cast<uint, byte>( Data.AsSpan() );
		return $"const {GetDataType( DataTypeId )} = {GetDataType( DataTypeId )?.Parse( data ) ?? $"0x{Convert.ToHexString( data )}"}";
	}
}

public class ConstantComposite : CompilerObject, IValue {
	public ConstantComposite ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint DataTypeId;
	public uint[] ValueIds = Array.Empty<uint>();

	public DataType Type => GetDataType( DataTypeId );

	public IVariable CreateVariable () {
		var type = Type.GetRuntimeType();
		var variable = type.CreateVariable();
		for ( uint i = 0; i < ValueIds.Length; i++ ) {
			IVariable parsed;
			if ( Compiler.Constants.TryGetValue( ValueIds[i], out var constant ) )
				parsed = constant.CreateVariable();
			else
				parsed = Compiler.CompositeConstants[ValueIds[i]].CreateVariable();

			((ICompositeVariable)variable)[i].Value = parsed.Value;
		}

		return variable;
	}

	public override string ToString () {
		var values = ValueIds.Select( GetValue );
		return $"const {GetDataType( DataTypeId )} = {{{string.Join(", ", values)}}}";
	}
}
