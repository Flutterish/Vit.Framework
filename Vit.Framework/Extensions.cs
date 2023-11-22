using System.Numerics;
using System.Runtime.CompilerServices;

namespace Vit.Framework;

public static class StructExtensions {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool TrySet<T> ( this T value, ref T field ) where T : struct, IEqualityOperators<T, T, bool> {
		if ( field == value )
			return false;

		field = value;
		return true;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool TrySet ( this bool value, ref bool field ) {
		if ( field == value )
			return false;

		field = value;
		return true;
	}
}

public static class ClassExtenions {
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool TrySet<T> ( this T value, ref T field ) where T : class? {
		if ( field == value )
			return false;

		field = value;
		return true;
	}
}