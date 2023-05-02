using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vit.Framework.Graphics.Software.Spirv.Instructions;
using Vit.Framework.Graphics.Software.Spirv.Metadata;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public abstract class DataType : CompilerObject {
	public DataType ( SpirvCompiler compiler ) : base( compiler ) { }

	public virtual object? Parse ( ReadOnlySpan<byte> data ) {
		return null;
	}
}