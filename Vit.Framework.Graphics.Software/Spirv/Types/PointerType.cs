﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public class PointerType : DataType {
	public PointerType ( SpirvCompiler compiler ) : base( compiler ) { }

	public StorageClass StorageClass;
	public uint TypeId;

	public override string ToString () {
		return $"{GetDataType(TypeId)}* ({StorageClass})";
	}
}
