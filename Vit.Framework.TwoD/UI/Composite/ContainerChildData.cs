using System.Runtime.CompilerServices;

namespace Vit.Framework.TwoD.UI.Composite;

public readonly struct ContainerChildData<T> : IChildData<T> where T : UIComponent {
	public required readonly T Child {
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] init; 
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static implicit operator ContainerChildData<T> ( T child )
		=> new() { Child = child };
}
