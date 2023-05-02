using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vortice.ShaderCompiler;

namespace Vit.Framework.Graphics.Software.Spirv.Instructions;

public class Store : Instruction {
	public Store ( SourceRef sourceRef ) : base( sourceRef ) { }

	public uint PointerId;
	public uint ObjectId;
	public MemoryOperands? MemoryOperands;

	public override string ToString () {
		return $"*({GetAssignable( PointerId )}) = {GetValue( ObjectId )}{( MemoryOperands == null ? "" : $" {MemoryOperands}" )}";
	}
}
