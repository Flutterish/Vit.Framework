using System.Runtime.CompilerServices;

namespace Vit.Framework.TwoD.UI.Composite;

public interface IChildData<T> where T : UIComponent {
	T Child { 
		[MethodImpl( MethodImplOptions.AggressiveInlining )] get;
		[MethodImpl( MethodImplOptions.AggressiveInlining )] init;
	}
}
