﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Constant : CompilerObject, IValue {
	public Constant ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint DataTypeId;
	public uint[] Data = Array.Empty<uint>();

	public override string ToString () {
		var data = MemoryMarshal.Cast<uint, byte>( Data.AsSpan() );
		return $"const {GetDataType( DataTypeId )} = {GetDataType( DataTypeId )?.Parse( data ) ?? $"0x{Convert.ToHexString( data )}"}";
	}
}

public class ConstantComposite : CompilerObject, IValue {
	public ConstantComposite ( SpirvCompiler compiler ) : base( compiler ) { }

	public uint DataTypeId;
	public uint[] ValueIds = Array.Empty<uint>();

	public override string ToString () {
		var values = ValueIds.Select( GetValue );
		return $"const {GetDataType( DataTypeId )} = {{{string.Join(", ", values)}}}";
	}
}
