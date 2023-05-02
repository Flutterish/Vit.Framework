using System.Diagnostics;
using Vit.Framework.Graphics.Software.Spirv.Metadata;
using Vit.Framework.Graphics.Software.Spirv.Runtime;

namespace Vit.Framework.Graphics.Software.Spirv.Types;

public abstract class DataType : CompilerObject {
	public DataType ( SpirvCompiler compiler, uint id ) : base( compiler, id ) { }

	public virtual object? Parse ( ReadOnlySpan<byte> data ) {
		return null;
	}

	IRuntimeType? runtimeType;
	public IRuntimeType GetRuntimeType () {
		return runtimeType ??= CreateRuntimeType();
	}
	protected virtual IRuntimeType CreateRuntimeType () {
		Debug.Fail( "oops" );
		return null!;
	}
}