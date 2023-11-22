using SharpGen.Runtime;
using System.Runtime.CompilerServices;

namespace Vit.Framework.Graphics.Direct3D11;

public static class D3DExtensions {
	public static void Validate ( this Result result, [CallerArgumentExpression(nameof(result))] string? expression = null ) {
		if ( !result.Success )
			throw new Exception( $"Operation failed: {result} at {expression} - {new SharpGenException( result )}" );
	}
}
