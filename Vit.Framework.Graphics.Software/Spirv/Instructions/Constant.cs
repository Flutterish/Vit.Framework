using System.Runtime.InteropServices;
using Vit.Framework.Graphics.Software.Shaders;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;
using Vit.Framework.Graphics.Software.Spirv.Types;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Constant : CompilerObject, IValue {
	public Constant ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public uint DataTypeId;
	public uint[] Data = Array.Empty<uint>();
	public ReadOnlySpan<byte> DataSpan => MemoryMarshal.Cast<uint, byte>( Data.AsSpan() );

	public DataType Type => GetDataType( DataTypeId );

	public void Load ( int address, ShaderMemory memory ) {
		MemoryMarshal.AsBytes( Data.AsSpan() ).CopyTo( memory.Memory[address..] );
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

	public void Load ( int address, ShaderMemory memory ) {
		var type = (ICompositeRuntimeType)Type.GetRuntimeType();
		for ( int i = 0; i < ValueIds.Length; i++ ) {
			var offset = type.GetMemberOffset( i );

			var id = ValueIds[i];
			if ( Compiler.Constants.TryGetValue( id, out var constant ) ) {
				constant.Load( address + offset, memory );
			}
			else {
				Compiler.CompositeConstants[id].Load( address + offset, memory );
			}
		}
	}

	public override string ToString () {
		var values = ValueIds.Select( GetValue );
		return $"const {GetDataType( DataTypeId )} = {{{string.Join(", ", values)}}}";
	}
}
