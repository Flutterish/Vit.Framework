﻿using SharpGen.Runtime;
using System.Runtime.CompilerServices;

namespace Vit.Framework.Graphics.Direct3D11;

public static class D3DExtensions {
	public static void Validate ( Result result, [CallerArgumentExpression(nameof(result))] string? expression = null ) {
		if ( result != Result.Ok )
			throw new Exception( $"Operation failed: {result} at {expression}" );
	}
}
