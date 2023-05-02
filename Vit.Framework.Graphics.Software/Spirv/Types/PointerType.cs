﻿using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class PointerType : DataType {
	public PointerType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public StorageClass StorageClass;
	public uint TypeId;
	public DataType Type => GetDataType( TypeId );

	protected override IRuntimeType CreateRuntimeType () {
		return new RuntimePointerType( Type.GetRuntimeType() );
	}

	public override string ToString () {
		return $"{GetDataType(TypeId)}* ({StorageClass})";
	}
}
