using System.Runtime.CompilerServices;

namespace Vit.Framework.TwoD.UI.Composite;

public struct ParametrizedChildData<T, TParam> : IChildData<T> where T : UIComponent where TParam : unmanaged {
	public required readonly T Child {
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] init;
	}
	public required TParam Parameter;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static implicit operator ParametrizedChildData<T, TParam> ( (T child, TParam param) data )
		=> new() { Child = data.child, Parameter = data.param };

	public void Deconstruct ( out T child, out TParam param ) {
		child = Child;
		param = Parameter;
	}
}
